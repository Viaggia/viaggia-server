using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.BillingPortal;
using Stripe.Checkout;
using Stripe.FinancialConnections;
using System.Security.Claims;
using viaggia_server.Data;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Payments;
using viaggia_server.DTOs.Reservation;
using viaggia_server.Models.Reserves;
using viaggia_server.Services.Payment;

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
        public async Task<IActionResult> CreatePaymentIntent([FromBody] ReservationCreateDTO createReservation)
        {
            try
            {
                var session = await _stripePaymentService.CreatePaymentIntentAsync(createReservation);

                if (session == null)
                {
                    return StatusCode(500, "Falha ao criar sessão de pagamento.");
                }

                return Ok(new { url = session.Url });
            }
            catch (Exception ex)
            {
                // Loga no console da aplicação e retorna erro detalhado para debug
                Console.WriteLine($"Erro ao criar pagamento: {ex.Message}");
                return StatusCode(500, $"Erro ao criar pagamento: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpPost("/webhook")]
        
        public async Task<IActionResult> Post()
        {
            await _stripePaymentService.HandleStripeWebhookAsync(Request);
            return Ok();
        }
    }
}
