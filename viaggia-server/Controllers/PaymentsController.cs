using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using viaggia_server.DTOs;
using viaggia_server.Services.Payment;
using viaggia_server.Data;
using viaggia_server.DTOs.Payments;
using Microsoft.EntityFrameworkCore;
using viaggia_server.Models.Reservations;
using Stripe;
using viaggia_server.DTOs.ReservationDTO;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IStripePaymentService _stripePaymentService;
        
        public PaymentsController(IStripePaymentService stripePaymentService, ILogger<PaymentsController> logger)
        {
            _stripePaymentService = stripePaymentService;
        }

        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreateReservation createReservation)
        {
            await _stripePaymentService.CreatePaymentIntentAsync(createReservation);
            return Ok();
        }
    }
}
