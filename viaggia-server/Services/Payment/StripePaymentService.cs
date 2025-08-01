using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Globalization;
using viaggia_server.DTOs.Reservation;
using viaggia_server.Models.Reservations;
using viaggia_server.Models.Users;
using viaggia_server.Repositories;

namespace viaggia_server.Services.Payment
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IRepository<Reservation> _reservations;
        private readonly ILogger<StripePaymentService> _logger;
        private readonly string _stripeSecretKey;
        private readonly string _stripeWebhookSecret;

        public StripePaymentService(
            IConfiguration configuration,
            IRepository<Reservation> reservations,
            ILogger<StripePaymentService> logger
        )
        {
            _configuration = configuration;
            _reservations = reservations;
            _logger = logger;
            _stripeSecretKey = _configuration["Stripe:SecretKey"];
            _stripeWebhookSecret = _configuration["Stripe:WebhookSecret"];
        }

        public async Task<Session> CreatePaymentIntentAsync(ReservationCreateDTO createReservation)
        {
            decimal total = createReservation.TotalPrice;
            try
            {
                StripeConfiguration.ApiKey = _stripeSecretKey;

                _logger.LogInformation("Iniciando criação de pagamento no Stripe");
                _logger.LogInformation("Stripe Secret Key: {Key}", _stripeSecretKey);
                _logger.LogInformation("DTO recebido: {@DTO}", createReservation);
                _logger.LogInformation("Valor total: {Total}", total);

                if (total <= 0)
                {
                    _logger.LogError("Valor total inválido: {Total}", total);
                    throw new ArgumentException("TotalPrice deve ser maior que zero.");
                }

                var productName = $"Reserva para o pacote {createReservation.PackageId} para o id {createReservation.UserId}";
                _logger.LogInformation("Nome do produto: {ProductName}", productName);

                var amount = (long)(total * 100);
                _logger.LogInformation("Valor que vai para priceOptions{amount}", amount);

                var priceOptions = new PriceCreateOptions
                {
                    UnitAmount = amount,
                    Currency = "brl",
                    ProductData = new PriceProductDataOptions
                    {
                        Name = productName
                    }
                };

                var priceService = new PriceService();
                var price = await priceService.CreateAsync(priceOptions);
                _logger.LogInformation("Preço criado: {PriceId}", price.Id);

                var sessionOptions = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = price.Id,
                    Quantity = createReservation.PackageId,
                }
            },
                    Mode = "payment",
                    SuccessUrl = $"http://localhost:5173/api/Reservation/{createReservation.UserId}",
                    CancelUrl = "http://localhost:5173/cancelado",
                    Metadata = new Dictionary<string, string>
            {
                { "userId", createReservation.UserId.ToString() },
                { "packageId", createReservation.PackageId.ToString() ?? "0" },
                { "roomTypeId", createReservation.RoomTypeId.ToString() ?? "0" },
                { "checkInDate", createReservation.CheckInDate.ToString("o") },
                { "checkOutDate", createReservation.CheckOutDate.ToString("o") },
                { "TotalPrice", total.ToString(CultureInfo.InvariantCulture) },
                { "status", createReservation.Status },
                { "numberOfGuests", createReservation.NumberOfGuests.ToString() }
            }
                };

                var sessionService = new SessionService();
                var session = await sessionService.CreateAsync(sessionOptions);

                _logger.LogInformation("Sessão de pagamento criada com sucesso: {SessionUrl}", session.Url);

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar sessão de pagamento no Stripe");
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
                        var reservation = new Reservation
                        {
                            UserId = int.Parse(session.Metadata["userId"]),
                            PackageId = int.Parse(session.Metadata["packageId"]),
                            RoomTypeId = int.Parse(session.Metadata["roomTypeId"]),
                            CheckInDate = DateTime.Parse(session.Metadata["checkInDate"], null, DateTimeStyles.RoundtripKind),
                            CheckOutDate = DateTime.Parse(session.Metadata["checkOutDate"], null, DateTimeStyles.RoundtripKind),
                            Status = session.Metadata["status"],
                            NumberOfGuests = int.Parse(session.Metadata["numberOfGuests"])
                        };

                        await _reservations.AddAsync(reservation);
                        await _reservations.SaveChangesAsync();

                        _logger.LogInformation($"Reserva criada com sucesso para o usuário {reservation.UserId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao salvar reserva do webhook.");
                    }
                }
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Erro na validação do webhook Stripe.");
                throw; // Você pode decidir se quer ou não relançar
            }
        }

    }
}
