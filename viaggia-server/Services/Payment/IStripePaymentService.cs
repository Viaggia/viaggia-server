using Stripe;
using viaggia_server.DTOs.Payment;

namespace viaggia_server.Services.Payment
{
    public interface IStripePaymentService
    {
        /// <summary>
        /// Cria um Payment Intent no Stripe para processar o pagamento
        /// </summary>
        Task<PaymentIntentResponseDTO> CreatePaymentIntentAsync(CreatePaymentIntentDTO request, int userId);

        /// <summary>
        /// Confirma um pagamento no Stripe
        /// </summary>
        Task<PaymentDTO> ConfirmPaymentAsync(ConfirmPaymentDTO request, int userId);

        /// <summary>
        /// Cancela um Payment Intent no Stripe
        /// </summary>
        Task<bool> CancelPaymentIntentAsync(string paymentIntentId);

        /// <summary>
        /// Cria um reembolso no Stripe
        /// </summary>
        Task<RefundResponseDTO> CreateRefundAsync(RefundDTO request);

        /// <summary>
        /// Busca um pagamento pelo Payment Intent ID
        /// </summary>
        Task<PaymentIntent?> GetPaymentIntentAsync(string paymentIntentId);

        /// <summary>
        /// Busca informações de um cliente no Stripe
        /// </summary>
        Task<Customer?> GetOrCreateCustomerAsync(int userId, string email, string name);

        /// <summary>
        /// Processa webhook do Stripe para atualizar status de pagamentos
        /// </summary>
        Task<bool> ProcessWebhookAsync(string json, string signature);

        /// <summary>
        /// Lista pagamentos de um usuário
        /// </summary>
        Task<List<PaymentDTO>> GetUserPaymentsAsync(int userId);

        /// <summary>
        /// Busca um pagamento por ID
        /// </summary>
        Task<PaymentDTO?> GetPaymentByIdAsync(int paymentId);

        /// <summary>
        /// Atualiza o status de um pagamento através do webhook
        /// </summary>
        Task<bool> UpdatePaymentStatusAsync(string paymentIntentId, string status, long amount);
    }
}
