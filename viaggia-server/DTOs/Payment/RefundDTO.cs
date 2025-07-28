namespace viaggia_server.DTOs.Payment
{
    public class RefundDTO
    {
        public int PaymentId { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public string Reason { get; set; } = "requested_by_customer";
        public string? Description { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
