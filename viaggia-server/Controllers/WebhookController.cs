using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.IO;
using Microsoft.Extensions.Configuration;
using viaggia_server.Services.Payment;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IStripePaymentService _stripePaymentService;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            IConfiguration configuration,
            IStripePaymentService stripePaymentService,
            ILogger<WebhookController> logger)
        {
            _configuration = configuration;
            _stripePaymentService = stripePaymentService;
            _logger = logger;
        }

        [HttpPost("stripe")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var endpointSecret = _configuration["Stripe:WebhookSecret"];

            try
            {
                // Verificar a assinatura do webhook
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    endpointSecret
                );

                _logger.LogInformation($"Webhook recebido: {stripeEvent.Type}");

                // Processar diferentes tipos de eventos
                switch (stripeEvent.Type)
                {
                    // 💰 EVENTOS DE PAGAMENTO
                    case "payment_intent.succeeded":
                        await HandlePaymentIntentSucceeded(stripeEvent);
                        break;

                    case "payment_intent.payment_failed":
                        await HandlePaymentIntentFailed(stripeEvent);
                        break;

                    case "payment_intent.canceled":
                        await HandlePaymentIntentCanceled(stripeEvent);
                        break;

                    // 🔄 EVENTOS DE REEMBOLSO
                    case "charge.refunded":
                        await HandleChargeRefunded(stripeEvent);
                        break;

                    case "refund.created":
                        await HandleRefundCreated(stripeEvent);
                        break;

                    // ⚠️ EVENTOS DE DISPUTA/CHARGEBACK
                    case "charge.dispute.created":
                        await HandleChargeDisputeCreated(stripeEvent);
                        break;

                    case "charge.dispute.updated":
                        await HandleChargeDisputeUpdated(stripeEvent);
                        break;

                    // 💳 EVENTOS DE CAPTURA DE PAGAMENTO
                    case "charge.captured":
                        await HandleChargeCaptured(stripeEvent);
                        break;

                    case "charge.failed":
                        await HandleChargeFailed(stripeEvent);
                        break;

                    // 🔐 EVENTOS DE AUTORIZAÇÃO
                    case "payment_intent.requires_action":
                        await HandlePaymentRequiresAction(stripeEvent);
                        break;

                    // 🌐 EVENTOS ESPECÍFICOS PARA TURISMO
                    case "payment_intent.processing":
                        await HandlePaymentProcessing(stripeEvent);
                        break;

                    default:
                        _logger.LogWarning($"Evento não processado: {stripeEvent.Type}");
                        break;
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Erro ao processar webhook do Stripe");
                return BadRequest($"Webhook error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao processar webhook");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task HandlePaymentIntentSucceeded(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            
            _logger.LogInformation($"💰 Pagamento confirmado: {paymentIntent.Id} - Valor: R$ {paymentIntent.Amount / 100:F2}");

            // Atualizar status do pagamento no banco de dados
            await _stripePaymentService.UpdatePaymentStatusAsync(
                paymentIntent.Id, 
                "succeeded", 
                paymentIntent.Amount
            );

            // 🏨 LÓGICA ESPECÍFICA PARA VIAGGIA:
            // - Confirmar reserva de hotel/pacote
            // - Enviar voucher por email
            // - Atualizar disponibilidade de quartos
            // - Notificar fornecedores (hotéis)
            // - Gerar comprovante fiscal
            // - Ativar seguro de viagem (se aplicável)
        }

        private async Task HandlePaymentIntentFailed(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            
            _logger.LogWarning($"❌ Pagamento falhou: {paymentIntent.Id} - Motivo: {paymentIntent.LastPaymentError?.Message}");

            await _stripePaymentService.UpdatePaymentStatusAsync(
                paymentIntent.Id, 
                "failed", 
                paymentIntent.Amount
            );

            // 🏨 LÓGICA ESPECÍFICA PARA VIAGGIA:
            // - Liberar quartos/vagas reservadas temporariamente
            // - Notificar usuário sobre falha no pagamento
            // - Oferecer métodos de pagamento alternativos
            // - Cancelar pré-reservas em hotéis
            // - Resetar timer de expiração de reserva
        }

        private async Task HandlePaymentIntentCanceled(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            
            _logger.LogInformation($"🚫 Pagamento cancelado: {paymentIntent.Id}");

            await _stripePaymentService.UpdatePaymentStatusAsync(
                paymentIntent.Id, 
                "canceled", 
                paymentIntent.Amount
            );

            // 🏨 LÓGICA ESPECÍFICA PARA VIAGGIA:
            // - Liberar quartos/vagas imediatamente
            // - Cancelar todas as pré-reservas associadas
            // - Notificar hotéis sobre cancelamento
            // - Remover da lista de espera
        }

        private async Task HandleChargeRefunded(Event stripeEvent)
        {
            var charge = stripeEvent.Data.Object as Charge;
            
            _logger.LogInformation($"💸 Reembolso processado: {charge.Id} - Valor: R$ {charge.AmountRefunded / 100:F2}");

            // 🏨 LÓGICA ESPECÍFICA PARA VIAGGIA:
            // - Cancelar reservas confirmadas
            // - Aplicar políticas de cancelamento (taxas)
            // - Notificar hotéis sobre cancelamento
            // - Atualizar disponibilidade de quartos
            // - Processar reembolso parcial se aplicável
            // - Cancelar serviços extras (transfer, passeios)
            
            await Task.CompletedTask; // Implementar lógica de reembolso
        }

        private async Task HandleRefundCreated(Event stripeEvent)
        {
            var refund = stripeEvent.Data.Object as Refund;
            
            _logger.LogInformation($"🔄 Reembolso criado: {refund.Id} - Status: {refund.Status}");

            // Acompanhar status do reembolso
            await Task.CompletedTask; // Implementar lógica de acompanhamento
        }

        private async Task HandleChargeDisputeCreated(Event stripeEvent)
        {
            var dispute = stripeEvent.Data.Object as Dispute;
            
            _logger.LogWarning($"⚠️ Disputa criada: {dispute.Id} - Motivo: {dispute.Reason}");

            // 🏨 LÓGICA ESPECÍFICA PARA TURISMO:
            // - Coletar comprovantes de hospedagem
            // - Reunir vouchers e confirmações
            // - Documentar check-in/check-out
            // - Coletar avaliações do cliente
            // - Notificar equipe jurídica
            // - Preparar defesa automática baseada no tipo de disputa
            
            await Task.CompletedTask; // Implementar lógica de disputa
        }

        private async Task HandleChargeDisputeUpdated(Event stripeEvent)
        {
            var dispute = stripeEvent.Data.Object as Dispute;
            
            _logger.LogInformation($"� Disputa atualizada: {dispute.Id} - Status: {dispute.Status}");

            // Acompanhar evolução da disputa
            await Task.CompletedTask;
        }

        private async Task HandleChargeCaptured(Event stripeEvent)
        {
            var charge = stripeEvent.Data.Object as Charge;
            
            _logger.LogInformation($"✅ Cobrança capturada: {charge.Id}");

            // Para reservas com autorização prévia
            // Confirmar definitivamente a reserva
            await Task.CompletedTask;
        }

        private async Task HandleChargeFailed(Event stripeEvent)
        {
            var charge = stripeEvent.Data.Object as Charge;
            
            _logger.LogWarning($"💥 Cobrança falhou: {charge.Id} - {charge.FailureMessage}");

            // Tentar novamente ou oferecer alternativas
            await Task.CompletedTask;
        }

        private async Task HandlePaymentRequiresAction(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            
            _logger.LogInformation($"� Pagamento requer ação: {paymentIntent.Id}");

            // 🏨 ESPECÍFICO PARA TURISMO:
            // - Notificar usuário sobre necessidade de autenticação 3D Secure
            // - Manter reserva temporária por tempo limitado
            // - Enviar link para completar autenticação
            
            await Task.CompletedTask;
        }

        private async Task HandlePaymentProcessing(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            
            _logger.LogInformation($"⏳ Pagamento em processamento: {paymentIntent.Id}");

            // Manter status de "aguardando confirmação"
            // Não liberar quartos ainda
            await Task.CompletedTask;
        }
    }
}
