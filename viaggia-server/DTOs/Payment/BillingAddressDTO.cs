using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Payment
{
    public class BillingAddressDTO
    {
        [Required(ErrorMessage = "Street is required.")]
        [StringLength(200, ErrorMessage = "Street cannot exceed 200 characters.")]
        public string Street { get; set; } = null!;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string City { get; set; } = null!;

        [Required(ErrorMessage = "State is required.")]
        [StringLength(50, ErrorMessage = "State cannot exceed 50 characters.")]
        public string State { get; set; } = null!;

        [Required(ErrorMessage = "Zip code is required.")]
        [StringLength(20, ErrorMessage = "Zip code cannot exceed 20 characters.")]
        public string ZipCode { get; set; } = null!;

        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
        public string Country { get; set; } = "Brazil";

        [StringLength(100, ErrorMessage = "Card holder name cannot exceed 100 characters.")]
        public string? CardHolderName { get; set; }
    }
}
