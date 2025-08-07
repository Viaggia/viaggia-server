using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using Stripe.Checkout;
using System.Globalization;
using System.Text.Json;
using viaggia_server.DTOs.Hotel;
using viaggia_server.DTOs.Reserves;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Reserves;
using viaggia_server.Models.Users;
using viaggia_server.Repositories;
using viaggia_server.Repositories.HotelRepository;
using viaggia_server.Repositories.Users;
using viaggia_server.Services.Email;

namespace viaggia_server.Services.Payment
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IHotelRepository _hotelRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IRepository<Reserve> _reservations;
        private readonly IRepository<Package> _package;
        private readonly ILogger<StripePaymentService> _logger;
        private readonly string _stripeSecretKey;
        private readonly string _stripeWebhookSecret;

        public StripePaymentService(
            IConfiguration configuration,
            IUserRepository userRepository,
            IHotelRepository hotelRepository,
            IEmailService emailService,
            IRepository<Reserve> reservations,
            IRepository<Package> package,
            ILogger<StripePaymentService> logger
        )
        {
            _configuration = configuration;
            _reservations = reservations;
            _package = package;
            _emailService = emailService;
            _hotelRepository = hotelRepository;
            _userRepository = userRepository;
            _logger = logger;
            _stripeSecretKey = _configuration["Stripe:SecretKey"];
            _stripeWebhookSecret = _configuration["Stripe:WebhookSecret"];
        }

        public async Task<Session> CreatePaymentIntentAsync(ReserveCreateDTO createReserve)
        {
            decimal total = createReserve.TotalPrice;
            try
            {
                StripeConfiguration.ApiKey = _stripeSecretKey;

                _logger.LogInformation("Iniciando cria��o de pagamento no Stripe");
                _logger.LogInformation("Stripe Secret Key: {Key}", _stripeSecretKey);
                _logger.LogInformation("DTO recebido: {@DTO}", createReserve);
                _logger.LogInformation("Valor total: {Total}", total);

                if (total <= 0)
                {
                    _logger.LogError("Valor total inv�lido: {Total}", total);
                    throw new ArgumentException("TotalPrice deve ser maior que zero.");
                }

                var name = await _userRepository.GetByIdAsync(createReserve.UserId);
                var package = await _package.GetByIdAsync(Convert.ToInt32(createReserve.PackageId));
                var description = package?.Description ?? "Descrição não disponível";

                var productName = $"Reserva para o cliente: {name}";
                _logger.LogInformation("Nome do produto: {ProductName}", productName);
                var amount = (long)(total * 100);
                _logger.LogInformation("Valor que vai para priceOptions {Amount}", amount);

                var priceOptions = new PriceCreateOptions
                {
                    UnitAmount = amount,
                    Currency = "brl",
                    ProductData = new PriceProductDataOptions
                    {
                        Name = productName,
                        StatementDescriptor = description,
                    }
                };

                var priceService = new PriceService();
                var price = await priceService.CreateAsync(priceOptions);
                _logger.LogInformation("Preço criado: {PriceId}", price.Id);

                // Serializa a lista de quartos (ReserveRooms) para JSON
                var reserveRoomsJson = JsonSerializer.Serialize(createReserve.ReserveRooms);

                var sessionOptions = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>
    {
        new SessionLineItemOptions
        {
            Price = price.Id,
            Quantity = 1,
        }
    },
                    Mode = "payment",
                    SuccessUrl = $"http://localhost:5173/paymentconfirmed",
                    CancelUrl = "http://localhost:5173/paymentcanceled",
                    Metadata = new Dictionary<string, string>
    {
        { "userId", createReserve.UserId.ToString() },
        { "packageId", createReserve.PackageId?.ToString() ?? "" },
        { "hotelId", createReserve.HotelId.ToString() },
        { "checkInDate", createReserve.CheckInDate.ToString("o") },
        { "checkOutDate", createReserve.CheckOutDate.ToString("o") },
        { "numberOfGuests", createReserve.NumberOfGuests.ToString() },
        { "status", createReserve.Status.ToString() },
        { "TotalPrice", total.ToString(CultureInfo.InvariantCulture) },
        { "reserveRooms", reserveRoomsJson }
    }
                };


                var sessionService = new SessionService();
                var session = await sessionService.CreateAsync(sessionOptions);
                _logger.LogInformation("Sess�o de pagamento criada com sucesso: {SessionUrl}", session.Url);
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar sess�o de pagamento no Stripe");
                throw; // ou return null, se preferir capturar no controller
            }
        }

        public async Task<Balance> GetBalanceAsync()
        {
            var service = new Stripe.BalanceService();
            var balance = service.Get();
            return balance;
        }

        public async Task<List<HotelBalanceDTO>> GetBalanceByHotelAsync()
        {
            try
            {
                return await _hotelRepository.GetBalancesHotelsAsync();
            } catch( Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task HandleStripeWebhookAsync(HttpRequest request)
        {
            var json = await new StreamReader(request.Body).ReadToEndAsync();
            var stripeSignature = request.Headers["Stripe-Signature"];
            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _stripeWebhookSecret, throwOnApiVersionMismatch: false);
                // _logger.LogInformation("Verifica: {stripeEventType}", stripeEvent.Type); -> Verifica se o pagamento foi realizado
                if (stripeEvent.Type.Equals("checkout.session.completed"))
                {
                    var session = stripeEvent.Data.Object as Session;
                    if (session == null)
                    {
                        _logger.LogError("Falha ao converter stripeEvent.Data.Object em Session");
                        return;
                    }

                    try
                    {
                        var reserveRoomsJson = session.Metadata["reserveRooms"];
                        var reserveRooms = JsonSerializer.Deserialize<List<ReserveRoom>>(reserveRoomsJson);

                        if (reserveRooms == null || !reserveRooms.Any())
                        {
                            _logger.LogError("Nenhum quarto informado na reserva.");
                            return;
                        }

                        var reservation = new Reserve
                        {
                            UserId = int.Parse(session.Metadata["userId"]),
                            PackageId = int.TryParse(session.Metadata["packageId"], out var packageId) ? packageId : null,
                            HotelId = int.Parse(session.Metadata["hotelId"]),
                            CheckInDate = DateTime.Parse(session.Metadata["checkInDate"], null, DateTimeStyles.RoundtripKind),
                            CheckOutDate = DateTime.Parse(session.Metadata["checkOutDate"], null, DateTimeStyles.RoundtripKind),
                            NumberOfGuests = int.Parse(session.Metadata["numberOfGuests"]),
                            Status = session.Metadata.ContainsKey("status") ? session.Metadata["status"] : "Pendente",
                            CreatedAt = DateTime.UtcNow,
                            TotalPrice = decimal.Parse(session.Metadata["TotalPrice"]),
                            ReserveRooms = reserveRooms
                        };

                        await _emailService.SendApprovedReserve(reservation);

                        await _reservations.AddAsync(reservation);
                        await _reservations.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao processar checkout.session.completed do Stripe");
                    }
                }

            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Erro na valida��o do webhook Stripe.");
                throw; // Voc� pode decidir se quer ou n�o relan�ar
            }
        }

    }
}