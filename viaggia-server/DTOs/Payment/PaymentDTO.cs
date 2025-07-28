namespace viaggia_server.DTOs.Payment
{
    public class PaymentDTO
    {
        public int PaymentId { get; set; }
        public int UserId { get; set; }
        public int ReservationId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? StripePaymentIntentId { get; set; }
        public string? StripePaymentMethodId { get; set; }
        public BillingAddressDTO? BillingAddress { get; set; }
        public bool IsActive { get; set; }
        
        // Informações adicionais do Stripe
        public string? TransactionId { get; set; }
        public string? Currency { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
