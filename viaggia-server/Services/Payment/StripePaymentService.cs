using System.Net;
using System.Text.Json;
using EllipticCurve.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using viaggia_server.Data;
using viaggia_server.DTOs.Payments;
using viaggia_server.DTOs.ReservationDTO;

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
        private readonly TokenService _tokenService;
        private readonly CustomerService _customerService;
        private readonly ChargeService _chargeService;
        private readonly ProductService _productService;

        public StripePaymentService(
            IConfiguration configuration,
            ILogger<StripePaymentService> logger,
            TokenService tokenService,
            CustomerService customerService,
            ChargeService chargeService,
            ProductService productService
            )
        {
            _configuration = configuration;
            _tokenService = tokenService;
            _customerService = customerService;
            _chargeService = chargeService;
            _productService = productService;
            _logger = logger;
            _stripeSecretKey = _configuration["Stripe:SecretKey"];
        }

        public async Task CreatePaymentIntentAsync(CreateReservation createReservation)
        {

            try
            {
                StripeConfiguration.ApiKey = _stripeSecretKey;

                var options = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Price = createReservation.TotalPrice,
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = "https://localhost:7164/home", //redirect page sucessfull
                    CancelUrl = "https://localhost:7164/home", //redirect page when isn't sucessfull
                };
                var service = new SessionService();

                Session session = service.Create(options);

                if (session.StripeResponse.StatusCode.Equals(200))
                {
                    var result = new Reservation
                    {
                        PackageId = createReservation.PackageId,
                        UserId = createReservation.UserId,
                        RoomTypeId = createReservation.RoomTypeId,
                        StartDate = createReservation.CheckInDate,
                        EndDate = createReservation.CheckOutDate,
                        Status = createReservation.Status,
                        NumberOfGuests = createReservation.NumberGuests
                    };
                    await _reservations.AddAsync(result);
                    // enviar email de confirma��oEmailService;
                }

                return ;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }
    }
}
