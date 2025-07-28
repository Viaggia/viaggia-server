namespace viaggia_server.DTOs.Payment
{
    public class PaymentIntentResponseDTO
    {
        public string PaymentIntentId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ReservationId { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
