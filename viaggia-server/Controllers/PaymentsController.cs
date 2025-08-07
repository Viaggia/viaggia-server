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
using viaggia_server.DTOs.Reserves;
using viaggia_server.Models.Reserves;
using viaggia_server.Services.Email;
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
        public async Task<IActionResult> CreatePaymentIntent([FromBody] ReserveCreateDTO createReservation)
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

        [HttpGet("Balance")]
        public async Task<IActionResult> GetBalance()
        {
            var balance = await _stripePaymentService.GetBalanceAsync();
            var result = new
            {
                Available = balance.Available.Select(a => new
                {
                    Amount = a.Amount,
                    Currency = a.Currency,
                }),
                Pending = balance.Pending.Select(p => new
                {
                    Amount = p.Amount,
                    Currency = p.Currency,
                })
            };
            return Ok(result);
        }

        [HttpGet("balance/hotels")]
        public async Task<IActionResult> GetBalanceByHtel()
        {
            try
            {
                var result = await _stripePaymentService.GetBalanceByHotelAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
