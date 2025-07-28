namespace viaggia_server.DTOs.Payment
{
    public class RefundDTO
    {
        public int PaymentId { get; set; }
        public decimal RefundAmount { get; set; }
        public string Reason { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class RefundResponseDTO
    {
        public string RefundId { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string? Description { get; set; }
    }
}
