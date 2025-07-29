using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Payment
{
    public class ConfirmPaymentDTO
    {
        [Required(ErrorMessage = "Payment Intent ID is required.")]
        public string PaymentIntentId { get; set; } = null!;

        [Required(ErrorMessage = "Payment Method ID is required.")]
        public string PaymentMethodId { get; set; } = null!;

        /// <summary>
        /// Token para autenticação 3D Secure se necessário
        /// </summary>
        public string? ThreeDSecureToken { get; set; }

        /// <summary>
        /// Informações adicionais do cartão se necessário
        /// </summary>
        public CardDetailsDTO? CardDetails { get; set; }
    }

    public class CardDetailsDTO
    {
        [StringLength(100, ErrorMessage = "Cardholder name cannot exceed 100 characters.")]
        public string? CardHolderName { get; set; }

        [StringLength(4, ErrorMessage = "Last 4 digits must be 4 characters.")]
        public string? Last4Digits { get; set; }

        [StringLength(50, ErrorMessage = "Brand cannot exceed 50 characters.")]
        public string? Brand { get; set; }

        public int? ExpiryMonth { get; set; }

        public int? ExpiryYear { get; set; }
    }
}
