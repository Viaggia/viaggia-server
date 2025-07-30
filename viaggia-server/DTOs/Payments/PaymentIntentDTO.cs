namespace viaggia_server.DTOs.Payments
{
    public class PaymentIntentDTO
    {
        /// <summary>
        /// Identificador do Payment Intent no Stripe
        /// </summary>
        public string Id { get; set; } = null!;
        /// <summary>
        /// Client Secret para confirmar o pagamento no frontend
        /// </summary>

        public string ClientSecret { get; set; } = null!;
        /// <summary>
        /// Status atual do Payment Intent
        /// </summary>
        public string Status { get; set; } = null!;

        public string Amount { get; set; } = null!;

        public string Currency { get; set; } = "brl"; // Default to Brazilian Real
    }
}
