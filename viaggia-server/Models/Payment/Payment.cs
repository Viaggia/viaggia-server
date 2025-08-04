using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using viaggia_server.Models.Reserves;
using viaggia_server.Models.Users;
using viaggia_server.Repositories;

namespace viaggia_server.Models.Payments
{
    public class Payment : ISoftDeletable
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        public int ReservationId { get; set; }

        [ForeignKey("ReservationId")]
        public virtual Reserve Reserve { get; set; } = null!;

        [Required(ErrorMessage = "Amount is required.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment date is required.")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "Payment method is required.")]
        [StringLength(50, ErrorMessage = "Payment method cannot exceed 50 characters.")]
        public string PaymentMethod { get; set; } = null!; // Ex.: "CreditCard", "BankTransfer", "Stripe"

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        public string Status { get; set; } = null!; // Ex.: "Completed", "Pending", "Failed", "Refunded"

        [Required(ErrorMessage = "Address is required.")]
        public int BillingAddressId { get; set; }

        [ForeignKey("BillingAddressId")]

        // ✅ Campos específicos do Stripe
        [StringLength(100, ErrorMessage = "Stripe Payment Intent ID cannot exceed 100 characters.")]
        public string? StripePaymentIntentId { get; set; }

        [StringLength(100, ErrorMessage = "Stripe Payment Method ID cannot exceed 100 characters.")]
        public string? StripePaymentMethodId { get; set; }

        [StringLength(100, ErrorMessage = "Stripe Customer ID cannot exceed 100 characters.")]
        public string? StripeCustomerId { get; set; }

        [StringLength(10, ErrorMessage = "Currency cannot exceed 10 characters.")]
        public string Currency { get; set; } = "BRL";

        [StringLength(500, ErrorMessage = "Transaction ID cannot exceed 500 characters.")]
        public string? TransactionId { get; set; }

        [StringLength(1000, ErrorMessage = "Metadata cannot exceed 1000 characters.")]
        public string? Metadata { get; set; } // JSON string for additional data

        [StringLength(1000, ErrorMessage = "Failure reason cannot exceed 1000 characters.")]
        public string? FailureReason { get; set; }

        public DateTime? RefundedAt { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? RefundAmount { get; set; }

        [StringLength(100, ErrorMessage = "Refund ID cannot exceed 100 characters.")]
        public string? StripeRefundId { get; set; }

        // Timestamps adicionais
        public DateTime? ProcessedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}