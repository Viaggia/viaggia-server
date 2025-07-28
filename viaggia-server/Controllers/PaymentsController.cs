using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Payment;
using viaggia_server.Services.Payment;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IStripePaymentService _stripePaymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IStripePaymentService stripePaymentService,
            ILogger<PaymentsController> logger)
        {
            _stripePaymentService = stripePaymentService;
            _logger = logger;
        }

        /// <summary>
        /// Cria um Payment Intent para iniciar o processo de pagamento
        /// </summary>
        [HttpPost("create-payment-intent")]
        [ProducesResponseType(typeof(ApiResponse<PaymentIntentResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentDTO request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new ApiResponse<object>(false, "User not authenticated"));

                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<object>(false, "Invalid request data", null, ModelState));

                var result = await _stripePaymentService.CreatePaymentIntentAsync(request, userId.Value);

                return Ok(new ApiResponse<PaymentIntentResponseDTO>(true, "Payment intent created successfully", result));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for payment intent creation");
                return BadRequest(new ApiResponse<object>(false, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error creating payment intent");
                return BadRequest(new ApiResponse<object>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating payment intent");
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error occurred"));
            }
        }

        /// <summary>
        /// Confirma um pagamento após o usuário inserir os dados do cartão
        /// </summary>
        [HttpPost("confirm-payment")]
        [ProducesResponseType(typeof(ApiResponse<PaymentDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentDTO request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new ApiResponse<object>(false, "User not authenticated"));

                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<object>(false, "Invalid request data", null, ModelState));

                var result = await _stripePaymentService.ConfirmPaymentAsync(request, userId.Value);

                if (result.Status == "RequiresAction")
                {
                    return Ok(new ApiResponse<PaymentDTO>(true, "Payment requires additional authentication", result));
                }

                return Ok(new ApiResponse<PaymentDTO>(true, "Payment confirmed successfully", result));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for payment confirmation");
                return BadRequest(new ApiResponse<object>(false, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error confirming payment");
                return BadRequest(new ApiResponse<object>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error confirming payment");
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error occurred"));
            }
        }

        /// <summary>
        /// Cancela um Payment Intent
        /// </summary>
        [HttpPost("cancel-payment-intent/{paymentIntentId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelPaymentIntent(string paymentIntentId)
        {
            try
            {
                if (string.IsNullOrEmpty(paymentIntentId))
                    return BadRequest(new ApiResponse<object>(false, "Payment Intent ID is required"));

                var success = await _stripePaymentService.CancelPaymentIntentAsync(paymentIntentId);

                if (success)
                    return Ok(new ApiResponse<object>(true, "Payment intent canceled successfully"));
                else
                    return BadRequest(new ApiResponse<object>(false, "Failed to cancel payment intent"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling payment intent {PaymentIntentId}", paymentIntentId);
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error occurred"));
            }
        }

        /// <summary>
        /// Cria um reembolso para um pagamento
        /// </summary>
        [HttpPost("refund")]
        [Authorize(Roles = "ADMIN,SERVICE_PROVIDER")]
        [ProducesResponseType(typeof(ApiResponse<RefundResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateRefund([FromBody] RefundDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<object>(false, "Invalid request data", null, ModelState));

                var result = await _stripePaymentService.CreateRefundAsync(request);

                return Ok(new ApiResponse<RefundResponseDTO>(true, "Refund created successfully", result));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for refund creation");
                return BadRequest(new ApiResponse<object>(false, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error creating refund");
                return BadRequest(new ApiResponse<object>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating refund");
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error occurred"));
            }
        }

        /// <summary>
        /// Lista todos os pagamentos do usuário autenticado
        /// </summary>
        [HttpGet("my-payments")]
        [ProducesResponseType(typeof(ApiResponse<List<PaymentDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyPayments()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new ApiResponse<object>(false, "User not authenticated"));

                var payments = await _stripePaymentService.GetUserPaymentsAsync(userId.Value);

                return Ok(new ApiResponse<List<PaymentDTO>>(true, "Payments retrieved successfully", payments));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user payments");
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error occurred"));
            }
        }

        /// <summary>
        /// Busca um pagamento específico por ID
        /// </summary>
        [HttpGet("{paymentId:int}")]
        [ProducesResponseType(typeof(ApiResponse<PaymentDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaymentById(int paymentId)
        {
            try
            {
                var payment = await _stripePaymentService.GetPaymentByIdAsync(paymentId);

                if (payment == null)
                    return NotFound(new ApiResponse<object>(false, "Payment not found"));

                // Verificar se o usuário tem permissão para ver este pagamento
                var userId = GetCurrentUserId();
                var userRoles = GetUserRoles();

                if (payment.UserId != userId && !userRoles.Contains("ADMIN"))
                    return Forbid("You don't have permission to view this payment");

                return Ok(new ApiResponse<PaymentDTO>(true, "Payment retrieved successfully", payment));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment {PaymentId}", paymentId);
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error occurred"));
            }
        }

        /// <summary>
        /// Webhook endpoint para receber eventos do Stripe
        /// </summary>
        [HttpPost("webhook")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> StripeWebhook()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var signature = Request.Headers["Stripe-Signature"].ToString();

                if (string.IsNullOrEmpty(signature))
                    return BadRequest("Missing Stripe signature");

                var success = await _stripePaymentService.ProcessWebhookAsync(json, signature);

                if (success)
                    return Ok("Webhook processed successfully");
                else
                    return BadRequest("Error processing webhook");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe webhook");
                return BadRequest("Error processing webhook");
            }
        }

        /// <summary>
        /// Busca informações sobre um Payment Intent específico
        /// </summary>
        [HttpGet("payment-intent/{paymentIntentId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaymentIntentStatus(string paymentIntentId)
        {
            try
            {
                if (string.IsNullOrEmpty(paymentIntentId))
                    return BadRequest(new ApiResponse<object>(false, "Payment Intent ID is required"));

                var paymentIntent = await _stripePaymentService.GetPaymentIntentAsync(paymentIntentId);

                if (paymentIntent == null)
                    return NotFound(new ApiResponse<object>(false, "Payment Intent not found"));

                var response = new
                {
                    Id = paymentIntent.Id,
                    Status = paymentIntent.Status,
                    Amount = paymentIntent.Amount / 100m,
                    Currency = paymentIntent.Currency.ToUpper(),
                    Created = paymentIntent.Created,
                    ClientSecret = paymentIntent.ClientSecret
                };

                return Ok(new ApiResponse<object>(true, "Payment Intent retrieved successfully", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment intent {PaymentIntentId}", paymentIntentId);
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error occurred"));
            }
        }

        #region Private Methods

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        private List<string> GetUserRoles()
        {
            return User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
        }

        #endregion
    }
}
