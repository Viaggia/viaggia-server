using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Payment
{
    public class CreatePaymentIntentDTO
    {
        [Required(ErrorMessage = "Reservation ID is required.")]
        public int ReservationId { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be 3 characters (e.g., USD, BRL).")]
        public string Currency { get; set; } = "BRL";

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        /// <summary>
        /// Endereço de cobrança para o pagamento
        /// </summary>
        public BillingAddressDTO BillingAddress { get; set; } = new BillingAddressDTO();

        /// <summary>
        /// Metadados personalizados para o pagamento
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
