using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Stripe;
using viaggia_server.Data;
using viaggia_server.DTOs.Payments;
using viaggia_server.Models.Addresses;
using viaggia_server.Models.Reservations;
using viaggia_server.Models.Users;
using viaggia_server.Repositories;

namespace viaggia_server.Services.Payment
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IRepository<Reservation> Reservations;
        private readonly ILogger<StripePaymentService> _logger;
        private readonly string _stripeSecretKey;

        public StripePaymentService(
            IConfiguration configuration,
            ILogger<StripePaymentService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _stripeSecretKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<PaymentIntentDTO> CreatePaymentIntentAsync(int id)
        {
            try
            {
                var reservation = await Reservations.GetByIdAsync(id);
                if (reservation == null)
                {
                    _logger.LogError("Reservation not found for ID: {Id}", id);
                    throw new KeyNotFoundException($"Reservation with ID {id} not found.");
                }
                
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(reservation.TotalPrice * 100), // Convert to cents
                    Currency = "brl", // Brazilian Real
                };

                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = await paymentIntentService.CreateAsync(options);

                return new PaymentIntentDTO
                {
                    Id = paymentIntent.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    Status = paymentIntent.Status,
                    Amount = (int)(reservation.TotalPrice * 100),
                    Currency = "brl"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent");
                throw;
            }
        }
    }
}
