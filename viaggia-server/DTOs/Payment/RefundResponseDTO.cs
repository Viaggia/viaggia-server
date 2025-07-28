namespace viaggia_server.DTOs.Payment
{
    public class RefundResponseDTO
    {
        public string RefundId { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "brl";
        public string Status { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        public string ChargeId { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
