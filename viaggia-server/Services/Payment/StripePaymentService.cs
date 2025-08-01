using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Stripe;
using viaggia_server.Data;
using viaggia_server.DTOs.Payment;
using viaggia_server.Models.Addresses;
using viaggia_server.Models.Payments;
using viaggia_server.Models.Users;
using viaggia_server.Repositories;

namespace viaggia_server.Services.Payment
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly IRepository<Models.Payments.Payment> _paymentRepository;
        private readonly ILogger<StripePaymentService> _logger;
        private readonly string _stripeSecretKey;
        private readonly string _webhookSecret;

        public StripePaymentService(
            IConfiguration configuration,
            AppDbContext context,
            IRepository<Models.Payments.Payment> paymentRepository,
            ILogger<StripePaymentService> logger)
        {
            _configuration = configuration;
            _context = context;
            _paymentRepository = paymentRepository;
            _logger = logger;
            
            _stripeSecretKey = _configuration["Stripe:SecretKey"] 
                ?? throw new InvalidOperationException("Stripe Secret Key not configured");
            _webhookSecret = _configuration["Stripe:WebhookSecret"] 
                ?? throw new InvalidOperationException("Stripe Webhook Secret not configured");
            
            StripeConfiguration.ApiKey = _stripeSecretKey;
        }

        public async Task<PaymentIntentResponseDTO> CreatePaymentIntentAsync(CreatePaymentIntentDTO request, int userId)
        {
            try
            {
                _logger.LogInformation("Creating payment intent for user {UserId}, amount {Amount}", userId, request.Amount);

                // Buscar usuário
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    throw new ArgumentException("User not found");

                // Buscar reserva
                var reservation = await _context.Reservations
                    .Include(r => r.Package)
                    .Include(r => r.Hotel)
                    .FirstOrDefaultAsync(r => r.ReservationId == request.ReservationId && r.UserId == userId);
                
                if (reservation == null)
                    throw new ArgumentException("Reservation not found or doesn't belong to user");

                // Criar ou buscar cliente no Stripe
                var customer = await GetOrCreateCustomerAsync(userId, user.Email, user.Name);

                // Preparar metadados
                var metadata = new Dictionary<string, string>
                {
                    ["user_id"] = userId.ToString(),
                    ["reservation_id"] = request.ReservationId.ToString(),
                    ["package_name"] = reservation.Package?.Name ?? "Direct Hotel Booking",
                    ["hotel_name"] = reservation.Hotel?.Name ?? "Unknown Hotel"
                };

                // Adicionar metadados customizados
                foreach (var item in request.Metadata)
                {
                    metadata[$"custom_{item.Key}"] = item.Value;
                }

                // Criar Payment Intent
                var paymentIntentService = new PaymentIntentService();
                var paymentIntentOptions = new PaymentIntentCreateOptions
                {
                    Amount = (long)(request.Amount * 100), // Stripe usa centavos
                    Currency = request.Currency.ToLower(),
                    Customer = customer.Id,
                    Description = request.Description ?? $"Payment for reservation #{request.ReservationId}",
                    Metadata = metadata,
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true
                    },
                    PaymentMethodTypes = new List<string> { "card" }
                };

                var paymentIntent = await paymentIntentService.CreateAsync(paymentIntentOptions);

                _logger.LogInformation("Payment intent created successfully: {PaymentIntentId}", paymentIntent.Id);

                return new PaymentIntentResponseDTO
                {
                    PaymentIntentId = paymentIntent.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    Status = paymentIntent.Status,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow,
                    ReservationId = request.ReservationId,
                    Metadata = metadata
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating payment intent");
                throw new InvalidOperationException($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent");
                throw;
            }
        }

        public async Task<PaymentDTO> ConfirmPaymentAsync(ConfirmPaymentDTO request, int userId)
        {
            try
            {
                _logger.LogInformation("Confirming payment for user {UserId}, PaymentIntent {PaymentIntentId}", 
                    userId, request.PaymentIntentId);

                var paymentIntentService = new PaymentIntentService();
                
                // Confirmar o Payment Intent
                var confirmOptions = new PaymentIntentConfirmOptions
                {
                    PaymentMethod = request.PaymentMethodId
                };

                var paymentIntent = await paymentIntentService.ConfirmAsync(request.PaymentIntentId, confirmOptions);

                // Se o pagamento requer autenticação adicional, retornar status
                if (paymentIntent.Status == "requires_action")
                {
                    return new PaymentDTO
                    {
                        StripePaymentIntentId = paymentIntent.Id,
                        Status = "RequiresAction",
                        Amount = paymentIntent.Amount / 100m,
                        Currency = paymentIntent.Currency.ToUpper()
                    };
                }

                // Se pagamento foi bem-sucedido, salvar no banco
                if (paymentIntent.Status == "succeeded")
                {
                    return await SaveSuccessfulPaymentAsync(paymentIntent, userId, request.PaymentMethodId);
                }

                // Se falhou
                throw new InvalidOperationException($"Payment failed with status: {paymentIntent.Status}");
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error confirming payment");
                throw new InvalidOperationException($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment");
                throw;
            }
        }

        public async Task<bool> CancelPaymentIntentAsync(string paymentIntentId)
        {
            try
            {
                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = await paymentIntentService.CancelAsync(paymentIntentId);
                
                return paymentIntent.Status == "canceled";
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error canceling payment intent {PaymentIntentId}", paymentIntentId);
                return false;
            }
        }

        public async Task<RefundResponseDTO> CreateRefundAsync(RefundDTO request)
        {
            try
            {
                // Buscar pagamento no banco
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.PaymentId == request.PaymentId && p.IsActive);

                if (payment == null || string.IsNullOrEmpty(payment.StripePaymentIntentId))
                    throw new ArgumentException("Payment not found or not eligible for refund");

                var refundService = new RefundService();
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = payment.StripePaymentIntentId,
                    Amount = (long)(request.RefundAmount * 100),
                    Reason = request.Reason,
                    Metadata = new Dictionary<string, string>
                    {
                        ["original_payment_id"] = payment.PaymentId.ToString(),
                        ["refund_reason"] = request.Reason,
                        ["description"] = request.Description ?? ""
                    }
                };

                var refund = await refundService.CreateAsync(refundOptions);

                // Atualizar pagamento no banco
                payment.Status = "Refunded";
                payment.RefundAmount = request.RefundAmount;
                payment.RefundedAt = DateTime.UtcNow;
                payment.StripeRefundId = refund.Id;

                await _context.SaveChangesAsync();

                return new RefundResponseDTO
                {
                    RefundId = refund.Id,
                    Status = refund.Status,
                    Amount = refund.Amount / 100m,
                    Currency = refund.Currency.ToUpper(),
                    Reason = refund.Reason,
                    CreatedAt = refund.Created,
                    Description = request.Description
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating refund");
                throw new InvalidOperationException($"Stripe error: {ex.Message}");
            }
        }

        public async Task<PaymentIntent?> GetPaymentIntentAsync(string paymentIntentId)
        {
            try
            {
                var paymentIntentService = new PaymentIntentService();
                return await paymentIntentService.GetAsync(paymentIntentId);
            }
            catch (StripeException)
            {
                return null;
            }
        }

        public async Task<Customer?> GetOrCreateCustomerAsync(int userId, string email, string name)
        {
            try
            {
                // Verificar se usuário já tem customer ID no Stripe
                var user = await _context.Users.FindAsync(userId);
                if (user != null && !string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    var customerService = new CustomerService();
                    try
                    {
                        return await customerService.GetAsync(user.StripeCustomerId);
                    }
                    catch (StripeException)
                    {
                        // Customer não existe mais, criar novo
                    }
                }

                // Criar novo customer
                var customerService2 = new CustomerService();
                var customerOptions = new CustomerCreateOptions
                {
                    Email = email,
                    Name = name,
                    Metadata = new Dictionary<string, string>
                    {
                        ["user_id"] = userId.ToString()
                    }
                };

                var customer = await customerService2.CreateAsync(customerOptions);

                // Salvar customer ID no usuário
                if (user != null)
                {
                    user.StripeCustomerId = customer.Id;
                    await _context.SaveChangesAsync();
                }

                return customer;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error creating Stripe customer");
                throw new InvalidOperationException($"Error creating customer: {ex.Message}");
            }
        }

        public async Task<bool> ProcessWebhookAsync(string json, string signature)
        {
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, signature, _webhookSecret);

                _logger.LogInformation("Processing Stripe webhook: {EventType}", stripeEvent.Type);

                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        await HandlePaymentSucceeded(stripeEvent);
                        break;
                    case "payment_intent.payment_failed":
                        await HandlePaymentFailed(stripeEvent);
                        break;
                    case "charge.dispute.created":
                        await HandleChargeDispute(stripeEvent);
                        break;
                    default:
                        _logger.LogInformation("Unhandled webhook event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return false;
            }
        }

        public async Task<List<PaymentDTO>> GetUserPaymentsAsync(int userId)
        {
            var payments = await _context.Payments
                .Include(p => p.BillingAddress)
                .Where(p => p.UserId == userId && p.IsActive)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return payments.Select(MapToPaymentDTO).ToList();
        }

        public async Task<PaymentDTO?> GetPaymentByIdAsync(int paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.BillingAddress)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId && p.IsActive);

            return payment != null ? MapToPaymentDTO(payment) : null;
        }

        #region Private Methods

        private async Task<PaymentDTO> SaveSuccessfulPaymentAsync(PaymentIntent paymentIntent, int userId, string paymentMethodId)
        {
            // Extrair dados dos metadados
            var reservationId = int.Parse(paymentIntent.Metadata["reservation_id"]);

            // Criar endereço de cobrança (básico)
            var billingAddress = new BillingAddress
            {
                Street = "Address from Stripe", // Implementar extração do endereço do payment method
                City = "City",
                State = "State",
                ZipCode = "00000-000",
                IsActive = true
            };

            _context.BillingAddresses.Add(billingAddress);
            await _context.SaveChangesAsync();

            // Criar registro de pagamento
            var payment = new Models.Payments.Payment
            {
                UserId = userId,
                ReservationId = reservationId,
                Amount = paymentIntent.Amount / 100m,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = "Stripe",
                Status = "Completed",
                StripePaymentIntentId = paymentIntent.Id,
                StripePaymentMethodId = paymentMethodId,
                Currency = paymentIntent.Currency.ToUpper(),
                TransactionId = paymentIntent.Id,
                Metadata = JsonSerializer.Serialize(paymentIntent.Metadata),
                BillingAddressId = billingAddress.AddressId,
                IsActive = true
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Atualizar status da reserva
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation != null)
            {
                reservation.Status = "Confirmed";
                await _context.SaveChangesAsync();
            }

            return MapToPaymentDTO(payment);
        }

        private async Task HandlePaymentSucceeded(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntent.Id);

            if (payment != null)
            {
                payment.Status = "Completed";
                payment.TransactionId = paymentIntent.Id;
                await _context.SaveChangesAsync();
            }
        }

        private async Task HandlePaymentFailed(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntent.Id);

            if (payment != null)
            {
                payment.Status = "Failed";
                payment.FailureReason = paymentIntent.LastPaymentError?.Message;
                await _context.SaveChangesAsync();
            }
        }

        private async Task HandleChargeDispute(Event stripeEvent)
        {
            // Implementar lógica para disputas de pagamento
            _logger.LogWarning("Charge dispute received: {EventId}", stripeEvent.Id);
            await Task.CompletedTask;
        }

        private PaymentDTO MapToPaymentDTO(Models.Payments.Payment payment)
        {
            var metadata = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(payment.Metadata))
            {
                try
                {
                    metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(payment.Metadata) 
                              ?? new Dictionary<string, string>();
                }
                catch
                {
                    // Ignorar erros de deserialização
                }
            }

            return new PaymentDTO
            {
                PaymentId = payment.PaymentId,
                UserId = payment.UserId,
                ReservationId = payment.ReservationId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                StripePaymentIntentId = payment.StripePaymentIntentId,
                StripePaymentMethodId = payment.StripePaymentMethodId,
                TransactionId = payment.TransactionId,
                Currency = payment.Currency,
                Metadata = metadata,
                IsActive = payment.IsActive,
                BillingAddress = payment.BillingAddress != null ? new BillingAddressDTO
                {
                    Street = payment.BillingAddress.Street,
                    City = payment.BillingAddress.City,
                    State = payment.BillingAddress.State,
                    ZipCode = payment.BillingAddress.ZipCode,
                    Country = "Brazil" // Default country since Address model doesn't have Country
                } : null
            };
        }

        public async Task<bool> UpdatePaymentStatusAsync(string paymentIntentId, string status, long amount)
        {
            try
            {
                _logger.LogInformation("Atualizando status do pagamento {PaymentIntentId} para {Status}", paymentIntentId, status);

                // Buscar o pagamento no banco de dados
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId);

                if (payment == null)
                {
                    _logger.LogWarning("Pagamento não encontrado para PaymentIntentId: {PaymentIntentId}", paymentIntentId);
                    return false;
                }

                // Atualizar o status
                payment.Status = MapStripeStatusToLocalStatus(status);
                payment.Amount = amount / 100m; // Converter de centavos para decimal
                payment.UpdatedAt = DateTime.UtcNow;

                // Se o pagamento foi bem-sucedido, definir data de processamento
                if (status == "succeeded")
                {
                    payment.ProcessedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Status do pagamento {PaymentIntentId} atualizado com sucesso para {Status}", paymentIntentId, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status do pagamento {PaymentIntentId}", paymentIntentId);
                return false;
            }
        }

        private string MapStripeStatusToLocalStatus(string stripeStatus)
        {
            return stripeStatus switch
            {
                "succeeded" => "Completed",
                "failed" => "Failed",
                "canceled" => "Cancelled",
                "processing" => "Processing",
                "requires_payment_method" => "Pending",
                "requires_confirmation" => "Pending",
                "requires_action" => "Pending",
                _ => "Unknown"
            };
        }

        #endregion
    }
}
