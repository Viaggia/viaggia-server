<<<<<<< HEAD
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

                _logger.LogInformation("Iniciando criaÁ„o de pagamento no Stripe");
                _logger.LogInformation("Stripe Secret Key: {Key}", _stripeSecretKey);
                _logger.LogInformation("DTO recebido: {@DTO}", createReservation);
                _logger.LogInformation("Valor total: {Total}", total);

                if (total <= 0)
                {
                    _logger.LogError("Valor total inv·lido: {Total}", total);
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
                _logger.LogInformation("PreÁo criado: {PriceId}", price.Id);

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

                _logger.LogInformation("Sess„o de pagamento criada com sucesso: {SessionUrl}", session.Url);

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar sess„o de pagamento no Stripe");
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

                        _logger.LogInformation($"Reserva criada com sucesso para o usu·rio {reservation.UserId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao salvar reserva do webhook.");
                    }
                }
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Erro na validaÁ„o do webhook Stripe.");
                throw; // VocÍ pode decidir se quer ou n„o relanÁar
            }
        }

    }
}
=======
//using System.Text.Json;
//using Microsoft.EntityFrameworkCore;
//using Stripe;
//using viaggia_server.Data;
//using viaggia_server.DTOs.Payment;
//using viaggia_server.Repositories;

//namespace viaggia_server.Services.Payment
//{
//    public class StripePaymentService : IStripePaymentService
//    {
//        private readonly IConfiguration _configuration;
//        private readonly AppDbContext _context;
//        private readonly IRepository<Models.Payments.Payment> _paymentRepository;
//        private readonly ILogger<StripePaymentService> _logger;
//        private readonly string _stripeSecretKey;
//        private readonly string _webhookSecret;

//        public StripePaymentService(
//            IConfiguration configuration,
//            AppDbContext context,
//            IRepository<Models.Payments.Payment> paymentRepository,
//            ILogger<StripePaymentService> logger)
//        {
//            _configuration = configuration;
//            _context = context;
//            _paymentRepository = paymentRepository;
//            _logger = logger;
            
//            _stripeSecretKey = _configuration["Stripe:SecretKey"] 
//                ?? throw new InvalidOperationException("Stripe Secret Key not configured");
//            _webhookSecret = _configuration["Stripe:WebhookSecret"] 
//                ?? throw new InvalidOperationException("Stripe Webhook Secret not configured");
            
//            StripeConfiguration.ApiKey = _stripeSecretKey;
//        }

//        public async Task<PaymentIntentResponseDTO> CreatePaymentIntentAsync(CreatePaymentIntentDTO request, int userId)
//        {
//            try
//            {
//                _logger.LogInformation("Creating payment intent for user {UserId}, amount {Amount}", userId, request.Amount);

//                // Buscar usu√°rio
//                var user = await _context.Users.FindAsync(userId);
//                if (user == null)
//                    throw new ArgumentException("User not found");

//                // Buscar reserva
//                var reservation = await _context.Reservations
//                    .Include(r => r.Package)
//                    .Include(r => r.Hotel)
//                    .FirstOrDefaultAsync(r => r.ReservationId == request.ReservationId && r.UserId == userId);
                
//                if (reservation == null)
//                    throw new ArgumentException("Reservation not found or doesn't belong to user");

//                // Criar ou buscar cliente no Stripe
//                var customer = await GetOrCreateCustomerAsync(userId, user.Email, user.Name);

//                // Preparar metadados
//                var metadata = new Dictionary<string, string>
//                {
//                    ["user_id"] = userId.ToString(),
//                    ["reservation_id"] = request.ReservationId.ToString(),
//                    ["package_name"] = reservation.Package?.Name ?? "Direct Hotel Booking",
//                    ["hotel_name"] = reservation.Hotel?.Name ?? "Unknown Hotel"
//                };

//                // Adicionar metadados customizados
//                foreach (var item in request.Metadata)
//                {
//                    metadata[$"custom_{item.Key}"] = item.Value;
//                }

//                // Criar Payment Intent
//                var paymentIntentService = new PaymentIntentService();
//                var paymentIntentOptions = new PaymentIntentCreateOptions
//                {
//                    Amount = (long)(request.Amount * 100), // Stripe usa centavos
//                    Currency = request.Currency.ToLower(),
//                    Customer = customer.Id,
//                    Description = request.Description ?? $"Payment for reservation #{request.ReservationId}",
//                    Metadata = metadata,
//                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
//                    {
//                        Enabled = true
//                    },
//                    PaymentMethodTypes = new List<string> { "card" }
//                };

//                var paymentIntent = await paymentIntentService.CreateAsync(paymentIntentOptions);

//                _logger.LogInformation("Payment intent created successfully: {PaymentIntentId}", paymentIntent.Id);

//                return new PaymentIntentResponseDTO
//                {
//                    PaymentIntentId = paymentIntent.Id,
//                    ClientSecret = paymentIntent.ClientSecret,
//                    Status = paymentIntent.Status,
//                    Amount = request.Amount,
//                    Currency = request.Currency,
//                    Description = request.Description,
//                    CreatedAt = DateTime.UtcNow,
//                    ReservationId = request.ReservationId,
//                    Metadata = metadata
//                };
//            }
//            catch (StripeException ex)
//            {
//                _logger.LogError(ex, "Stripe error creating payment intent");
//                throw new InvalidOperationException($"Stripe error: {ex.Message}");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error creating payment intent");
//                throw;
//            }
//        }

//        public async Task<PaymentDTO> ConfirmPaymentAsync(ConfirmPaymentDTO request, int userId)
//        {
//            try
//            {
//                _logger.LogInformation("Confirming payment for user {UserId}, PaymentIntent {PaymentIntentId}", 
//                    userId, request.PaymentIntentId);

//                var paymentIntentService = new PaymentIntentService();
                
//                // Confirmar o Payment Intent
//                var confirmOptions = new PaymentIntentConfirmOptions
//                {
//                    PaymentMethod = request.PaymentMethodId
//                };

//                var paymentIntent = await paymentIntentService.ConfirmAsync(request.PaymentIntentId, confirmOptions);

//                // Se o pagamento requer autentica√ß√£o adicional, retornar status
//                if (paymentIntent.Status == "requires_action")
//                {
//                    return new PaymentDTO
//                    {
//                        StripePaymentIntentId = paymentIntent.Id,
//                        Status = "RequiresAction",
//                        Amount = paymentIntent.Amount / 100m,
//                        Currency = paymentIntent.Currency.ToUpper()
//                    };
//                }

//                // Se pagamento foi bem-sucedido, salvar no banco
//                if (paymentIntent.Status == "succeeded")
//                {
//                    return await SaveSuccessfulPaymentAsync(paymentIntent, userId, request.PaymentMethodId);
//                }

//                // Se falhou
//                throw new InvalidOperationException($"Payment failed with status: {paymentIntent.Status}");
//            }
//            catch (StripeException ex)
//            {
//                _logger.LogError(ex, "Stripe error confirming payment");
//                throw new InvalidOperationException($"Stripe error: {ex.Message}");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error confirming payment");
//                throw;
//            }
//        }

//        public async Task<bool> CancelPaymentIntentAsync(string paymentIntentId)
//        {
//            try
//            {
//                var paymentIntentService = new PaymentIntentService();
//                var paymentIntent = await paymentIntentService.CancelAsync(paymentIntentId);
                
//                return paymentIntent.Status == "canceled";
//            }
//            catch (StripeException ex)
//            {
//                _logger.LogError(ex, "Error canceling payment intent {PaymentIntentId}", paymentIntentId);
//                return false;
//            }
//        }

//        public async Task<RefundResponseDTO> CreateRefundAsync(RefundDTO request)
//        {
//            try
//            {
//                // Buscar pagamento no banco
//                var payment = await _context.Payments
//                    .FirstOrDefaultAsync(p => p.PaymentId == request.PaymentId && p.IsActive);

//                if (payment == null || string.IsNullOrEmpty(payment.StripePaymentIntentId))
//                    throw new ArgumentException("Payment not found or not eligible for refund");

//                var refundService = new RefundService();
//                var refundOptions = new RefundCreateOptions
//                {
//                    PaymentIntent = payment.StripePaymentIntentId,
//                    Amount = (long)(request.RefundAmount * 100),
//                    Reason = request.Reason,
//                    Metadata = new Dictionary<string, string>
//                    {
//                        ["original_payment_id"] = payment.PaymentId.ToString(),
//                        ["refund_reason"] = request.Reason,
//                        ["description"] = request.Description ?? ""
//                    }
//                };

//                var refund = await refundService.CreateAsync(refundOptions);

//                // Atualizar pagamento no banco
//                payment.Status = "Refunded";
//                payment.RefundAmount = request.RefundAmount;
//                payment.RefundedAt = DateTime.UtcNow;
//                payment.StripeRefundId = refund.Id;

//                await _context.SaveChangesAsync();

//                return new RefundResponseDTO
//                {
//                    RefundId = refund.Id,
//                    Status = refund.Status,
//                    Amount = refund.Amount / 100m,
//                    Currency = refund.Currency.ToUpper(),
//                    Reason = refund.Reason,
//                    CreatedAt = refund.Created,
//                    Description = request.Description
//                };
//            }
//            catch (StripeException ex)
//            {
//                _logger.LogError(ex, "Stripe error creating refund");
//                throw new InvalidOperationException($"Stripe error: {ex.Message}");
//            }
//        }

//        public async Task<PaymentIntent?> GetPaymentIntentAsync(string paymentIntentId)
//        {
//            try
//            {
//                var paymentIntentService = new PaymentIntentService();
//                return await paymentIntentService.GetAsync(paymentIntentId);
//            }
//            catch (StripeException)
//            {
//                return null;
//            }
//        }

//        public async Task<Customer?> GetOrCreateCustomerAsync(int userId, string email, string name)
//        {
//            try
//            {
//                // Verificar se usu√°rio j√° tem customer ID no Stripe
//                var user = await _context.Users.FindAsync(userId);
//                if (user != null && !string.IsNullOrEmpty(user.StripeCustomerId))
//                {
//                    var customerService = new CustomerService();
//                    try
//                    {
//                        return await customerService.GetAsync(user.StripeCustomerId);
//                    }
//                    catch (StripeException)
//                    {
//                        // Customer n√£o existe mais, criar novo
//                    }
//                }

//                // Criar novo customer
//                var customerService2 = new CustomerService();
//                var customerOptions = new CustomerCreateOptions
//                {
//                    Email = email,
//                    Name = name,
//                    Metadata = new Dictionary<string, string>
//                    {
//                        ["user_id"] = userId.ToString()
//                    }
//                };

//                var customer = await customerService2.CreateAsync(customerOptions);

//                // Salvar customer ID no usu√°rio
//                if (user != null)
//                {
//                    user.StripeCustomerId = customer.Id;
//                    await _context.SaveChangesAsync();
//                }

//                return customer;
//            }
//            catch (StripeException ex)
//            {
//                _logger.LogError(ex, "Error creating Stripe customer");
//                throw new InvalidOperationException($"Error creating customer: {ex.Message}");
//            }
//        }

//        public async Task<bool> ProcessWebhookAsync(string json, string signature)
//        {
//            try
//            {
//                var stripeEvent = EventUtility.ConstructEvent(json, signature, _webhookSecret);

//                _logger.LogInformation("Processing Stripe webhook: {EventType}", stripeEvent.Type);

//                switch (stripeEvent.Type)
//                {
//                    case "payment_intent.succeeded":
//                        await HandlePaymentSucceeded(stripeEvent);
//                        break;
//                    case "payment_intent.payment_failed":
//                        await HandlePaymentFailed(stripeEvent);
//                        break;
//                    case "charge.dispute.created":
//                        await HandleChargeDispute(stripeEvent);
//                        break;
//                    default:
//                        _logger.LogInformation("Unhandled webhook event type: {EventType}", stripeEvent.Type);
//                        break;
//                }

//                return true;
//            }
//            catch (StripeException ex)
//            {
//                _logger.LogError(ex, "Error processing webhook");
//                return false;
//            }
//        }

//        public async Task<List<PaymentDTO>> GetUserPaymentsAsync(int userId)
//        {
//            var payments = await _context.Payments
//                .Include(p => p.BillingAddress)
//                .Where(p => p.UserId == userId && p.IsActive)
//                .OrderByDescending(p => p.PaymentDate)
//                .ToListAsync();

//            return payments.Select(MapToPaymentDTO).ToList();
//        }

//        public async Task<PaymentDTO?> GetPaymentByIdAsync(int paymentId)
//        {
//            var payment = await _context.Payments
//                .Include(p => p.BillingAddress)
//                .FirstOrDefaultAsync(p => p.PaymentId == paymentId && p.IsActive);

//            return payment != null ? MapToPaymentDTO(payment) : null;
//        }

//        #region Private Methods

//        private async Task<PaymentDTO> SaveSuccessfulPaymentAsync(PaymentIntent paymentIntent, int userId, string paymentMethodId)
//        {
//            // Extrair dados dos metadados
//            var reservationId = int.Parse(paymentIntent.Metadata["reservation_id"]);

//            // Criar endere√ßo de cobran√ßa (b√°sico)
//            var billingAddress = new BillingAddress
//            {
//                Street = "Address from Stripe", // Implementar extra√ß√£o do endere√ßo do payment method
//                City = "City",
//                State = "State",
//                ZipCode = "00000-000",
//                IsActive = true
//            };

//            _context.BillingAddresses.Add(billingAddress);
//            await _context.SaveChangesAsync();

//            // Criar registro de pagamento
//            var payment = new Models.Payments.Payment
//            {
//                UserId = userId,
//                ReservationId = reservationId,
//                Amount = paymentIntent.Amount / 100m,
//                PaymentDate = DateTime.UtcNow,
//                PaymentMethod = "Stripe",
//                Status = "Completed",
//                StripePaymentIntentId = paymentIntent.Id,
//                StripePaymentMethodId = paymentMethodId,
//                Currency = paymentIntent.Currency.ToUpper(),
//                TransactionId = paymentIntent.Id,
//                Metadata = JsonSerializer.Serialize(paymentIntent.Metadata),
//                BillingAddressId = billingAddress.AddressId,
//                IsActive = true
//            };

//            _context.Payments.Add(payment);
//            await _context.SaveChangesAsync();

//            // Atualizar status da reserva
//            var reservation = await _context.Reservations.FindAsync(reservationId);
//            if (reservation != null)
//            {
//                reservation.Status = "Confirmed";
//                await _context.SaveChangesAsync();
//            }

//            return MapToPaymentDTO(payment);
//        }

//        private async Task HandlePaymentSucceeded(Event stripeEvent)
//        {
//            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
//            if (paymentIntent == null) return;

//            var payment = await _context.Payments
//                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntent.Id);

//            if (payment != null)
//            {
//                payment.Status = "Completed";
//                payment.TransactionId = paymentIntent.Id;
//                await _context.SaveChangesAsync();
//            }
//        }

//        private async Task HandlePaymentFailed(Event stripeEvent)
//        {
//            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
//            if (paymentIntent == null) return;

//            var payment = await _context.Payments
//                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntent.Id);

//            if (payment != null)
//            {
//                payment.Status = "Failed";
//                payment.FailureReason = paymentIntent.LastPaymentError?.Message;
//                await _context.SaveChangesAsync();
//            }
//        }

//        private async Task HandleChargeDispute(Event stripeEvent)
//        {
//            // Implementar l√≥gica para disputas de pagamento
//            _logger.LogWarning("Charge dispute received: {EventId}", stripeEvent.Id);
//            await Task.CompletedTask;
//        }

//        private PaymentDTO MapToPaymentDTO(Models.Payments.Payment payment)
//        {
//            var metadata = new Dictionary<string, string>();
//            if (!string.IsNullOrEmpty(payment.Metadata))
//            {
//                try
//                {
//                    metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(payment.Metadata) 
//                              ?? new Dictionary<string, string>();
//                }
//                catch
//                {
//                    // Ignorar erros de deserializa√ß√£o
//                }
//            }

//            return new PaymentDTO
//            {
//                PaymentId = payment.PaymentId,
//                UserId = payment.UserId,
//                ReservationId = payment.ReservationId,
//                Amount = payment.Amount,
//                PaymentDate = payment.PaymentDate,
//                PaymentMethod = payment.PaymentMethod,
//                Status = payment.Status,
//                StripePaymentIntentId = payment.StripePaymentIntentId,
//                StripePaymentMethodId = payment.StripePaymentMethodId,
//                TransactionId = payment.TransactionId,
//                Currency = payment.Currency,
//                Metadata = metadata,
//                IsActive = payment.IsActive,
//                BillingAddress = payment.BillingAddress != null ? new BillingAddressDTO
//                {
//                    Street = payment.BillingAddress.Street,
//                    City = payment.BillingAddress.City,
//                    State = payment.BillingAddress.State,
//                    ZipCode = payment.BillingAddress.ZipCode,
//                    Country = "Brazil" // Default country since Address model doesn't have Country
//                } : null
//            };
//        }

//        public async Task<bool> UpdatePaymentStatusAsync(string paymentIntentId, string status, long amount)
//        {
//            try
//            {
//                _logger.LogInformation("Atualizando status do pagamento {PaymentIntentId} para {Status}", paymentIntentId, status);

//                // Buscar o pagamento no banco de dados
//                var payment = await _context.Payments
//                    .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId);

//                if (payment == null)
//                {
//                    _logger.LogWarning("Pagamento n√£o encontrado para PaymentIntentId: {PaymentIntentId}", paymentIntentId);
//                    return false;
//                }

//                // Atualizar o status
//                payment.Status = MapStripeStatusToLocalStatus(status);
//                payment.Amount = amount / 100m; // Converter de centavos para decimal
//                payment.UpdatedAt = DateTime.UtcNow;

//                // Se o pagamento foi bem-sucedido, definir data de processamento
//                if (status == "succeeded")
//                {
//                    payment.ProcessedAt = DateTime.UtcNow;
//                }

//                await _context.SaveChangesAsync();

//                _logger.LogInformation("Status do pagamento {PaymentIntentId} atualizado com sucesso para {Status}", paymentIntentId, status);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Erro ao atualizar status do pagamento {PaymentIntentId}", paymentIntentId);
//                return false;
//            }
//        }

//        private string MapStripeStatusToLocalStatus(string stripeStatus)
//        {
//            return stripeStatus switch
//            {
//                "succeeded" => "Completed",
//                "failed" => "Failed",
//                "canceled" => "Cancelled",
//                "processing" => "Processing",
//                "requires_payment_method" => "Pending",
//                "requires_confirmation" => "Pending",
//                "requires_action" => "Pending",
//                _ => "Unknown"
//            };
//        }

//        #endregion
//    }
//}
>>>>>>> 4ab8ac3dc4732ca91d9c662fc8b90e047b46890d
