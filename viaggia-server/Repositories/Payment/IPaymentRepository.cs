using viaggia_server.Models.Payments;

namespace viaggia_server.Repositories.Payment
{
    public interface IPaymentRepository : IRepository<Models.Payments.Payment>
    {
        /// <summary>
        /// Busca pagamentos por usuário com paginação
        /// </summary>
        Task<IEnumerable<Models.Payments.Payment>> GetPaymentsByUserIdAsync(int userId, int page = 1, int pageSize = 10);

        /// <summary>
        /// Busca pagamentos por reserva
        /// </summary>
        Task<IEnumerable<Models.Payments.Payment>> GetPaymentsByReservationIdAsync(int reservationId);

        /// <summary>
        /// Busca pagamento pelo Stripe Payment Intent ID
        /// </summary>
        Task<Models.Payments.Payment?> GetPaymentByStripeIntentIdAsync(string stripePaymentIntentId);

        /// <summary>
        /// Busca pagamentos por status
        /// </summary>
        Task<IEnumerable<Models.Payments.Payment>> GetPaymentsByStatusAsync(string status);

        /// <summary>
        /// Busca pagamentos em um período específico
        /// </summary>
        Task<IEnumerable<Models.Payments.Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Calcula total de pagamentos de um usuário
        /// </summary>
        Task<decimal> GetTotalPaymentsByUserIdAsync(int userId);

        /// <summary>
        /// Busca pagamentos que precisam de reembolso
        /// </summary>
        Task<IEnumerable<Models.Payments.Payment>> GetRefundablePaymentsAsync();

        /// <summary>
        /// Atualiza status de um pagamento
        /// </summary>
        Task<bool> UpdatePaymentStatusAsync(int paymentId, string status, string? failureReason = null);

        /// <summary>
        /// Busca estatísticas de pagamentos
        /// </summary>
        Task<PaymentStatistics> GetPaymentStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }

    public class PaymentStatistics
    {
        public int TotalPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public int CompletedPayments { get; set; }
        public int PendingPayments { get; set; }
        public int FailedPayments { get; set; }
        public int RefundedPayments { get; set; }
        public decimal AveragePaymentAmount { get; set; }
    }
}