using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Payments;

namespace viaggia_server.Repositories.Payment
{
    public class PaymentRepository : Repository<Models.Payments.Payment>, IPaymentRepository
    {
        public PaymentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Models.Payments.Payment>> GetPaymentsByUserIdAsync(int userId, int page = 1, int pageSize = 10)
        {
            return await _context.Payments
                .Include(p => p.BillingAddress)
                .Include(p => p.Reservation)
                .Where(p => p.UserId == userId && p.IsActive)
                .OrderByDescending(p => p.PaymentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Models.Payments.Payment>> GetPaymentsByReservationIdAsync(int reservationId)
        {
            return await _context.Payments
                .Include(p => p.BillingAddress)
                .Include(p => p.User)
                .Where(p => p.ReservationId == reservationId && p.IsActive)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Models.Payments.Payment?> GetPaymentByStripeIntentIdAsync(string stripePaymentIntentId)
        {
            return await _context.Payments
                .Include(p => p.BillingAddress)
                .Include(p => p.User)
                .Include(p => p.Reservation)
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == stripePaymentIntentId && p.IsActive);
        }

        public async Task<IEnumerable<Models.Payments.Payment>> GetPaymentsByStatusAsync(string status)
        {
            return await _context.Payments
                .Include(p => p.BillingAddress)
                .Include(p => p.User)
                .Include(p => p.Reservation)
                .Where(p => p.Status == status && p.IsActive)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Models.Payments.Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Include(p => p.BillingAddress)
                .Include(p => p.User)
                .Include(p => p.Reservation)
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate && p.IsActive)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPaymentsByUserIdAsync(int userId)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId && p.Status == "Completed" && p.IsActive)
                .SumAsync(p => p.Amount);
        }

        public async Task<IEnumerable<Models.Payments.Payment>> GetRefundablePaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.BillingAddress)
                .Include(p => p.User)
                .Include(p => p.Reservation)
                .Where(p => p.Status == "Completed" && 
                           p.RefundedAt == null && 
                           p.IsActive &&
                           !string.IsNullOrEmpty(p.StripePaymentIntentId))
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, string status, string? failureReason = null)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null || !payment.IsActive)
                return false;

            payment.Status = status;
            if (!string.IsNullOrEmpty(failureReason))
                payment.FailureReason = failureReason;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PaymentStatistics> GetPaymentStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Payments.Where(p => p.IsActive);

            if (startDate.HasValue)
                query = query.Where(p => p.PaymentDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.PaymentDate <= endDate.Value);

            var payments = await query.ToListAsync();

            return new PaymentStatistics
            {
                TotalPayments = payments.Count,
                TotalAmount = payments.Sum(p => p.Amount),
                CompletedPayments = payments.Count(p => p.Status == "Completed"),
                PendingPayments = payments.Count(p => p.Status == "Pending"),
                FailedPayments = payments.Count(p => p.Status == "Failed"),
                RefundedPayments = payments.Count(p => p.Status == "Refunded"),
                AveragePaymentAmount = payments.Any() ? payments.Average(p => p.Amount) : 0
            };
        }
    }
}
