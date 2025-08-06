using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Globalization;
using viaggia_server.DTOs.Reserves;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Reserves;
using viaggia_server.Models.Users;
using viaggia_server.Repositories;
using viaggia_server.Repositories.Users;
using viaggia_server.Services.Email;

namespace viaggia_server.Services.Payment
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IRepository<Reserve> _reservations;
        private readonly ILogger<StripePaymentService> _logger;
        private readonly string _stripeSecretKey;
        private readonly string _stripeWebhookSecret;

        public StripePaymentService(
            IConfiguration configuration,
            IUserRepository userRepository,
            IRepository<Reserve> reservations,
            ILogger<StripePaymentService> logger
        )
        {
            _configuration = configuration;
            _reservations = reservations;
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

                var productName = $"Reserva para o id {createReserve.UserId}";
                _logger.LogInformation("Nome do produto: {ProductName}", productName);

                var amount = (long)(total * 100);
                _logger.LogInformation("Valor que vai para priceOptions{amount}", amount);

                var priceOptions = new PriceCreateOptions
                {
                    UnitAmount = amount,
                    Currency = "brl",
                    ProductData = new PriceProductDataOptions
                    {
                        Name = productName,
                    }
                };

                var priceService = new PriceService();
                var price = await priceService.CreateAsync(priceOptions);
                _logger.LogInformation("Pre�o criado: {PriceId}", price.Id);

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
                { "packageId", createReserve.PackageId.ToString() ?? "" },
                { "roomTypeId", createReserve.RoomTypeId.ToString() },
                { "hotelId", createReserve.HotelId.ToString() },
                { "checkInDate", createReserve.CheckInDate.ToString("o") },
                { "checkOutDate", createReserve.CheckOutDate.ToString( "o") },
                { "numberOfGuests", createReserve.NumberOfGuests.ToString() },
                { "status", createReserve.Status.ToString() },
                { "TotalPrice", total.ToString(CultureInfo.InvariantCulture) },
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
                        var reservation = new Reserve
                        {
                            UserId = int.Parse(session.Metadata["userId"]),
                            PackageId = int.Parse(session.Metadata["packageId"]),
                            RoomTypeId = int.Parse(session.Metadata["roomTypeId"]),
                            HotelId = int.Parse(session.Metadata["hotelId"]),
                            CheckInDate = DateTime.Parse(session.Metadata["checkInDate"], null, DateTimeStyles.RoundtripKind),
                            CheckOutDate = DateTime.Parse(session.Metadata["checkOutDate"], null, DateTimeStyles.RoundtripKind),
                            NumberOfGuests = int.Parse(session.Metadata["numberOfGuests"]),
                            Status = session.Metadata.ContainsKey("status") ? session.Metadata["status"] : "Pendente",
                            CreatedAt = DateTime.UtcNow,
                            TotalPrice = decimal.Parse(session.Metadata["TotalPrice"]),
                        };

                        await _reservations.AddAsync(reservation);
                        await _reservations.SaveChangesAsync();

                        _logger.LogInformation($"Reserva criada com sucesso para o usuário {reservation.UserId}");

                        var user = await _userRepository.GetByIdAsync(reservation.UserId);
                        var hotel = await _reservations.GetByIdAsync<Hotel>(Convert.ToInt32(reservation.HotelId));

                        string userName = session.Metadata["userNameReservation"];
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao processar reserva no webhook Stripe.");
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