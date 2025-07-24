using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Users
{
    public class CreateClientDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\+?\d{10,15}$", ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "CPF is required.")]
        [RegularExpression(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", ErrorMessage = "Invalid CPF format (e.g., 123.456.789-00).")]
        public string Cpf { get; set; } = null!;

        [StringLength(200, ErrorMessage = "Street cannot exceed 200 characters.")]
        public string? AddressStreet { get; set; }

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string? AddressCity { get; set; }

        [StringLength(2, MinimumLength = 2, ErrorMessage = "State must be 2 characters (e.g., SP).")]
        public string? AddressState { get; set; }

        [RegularExpression(@"^\d{5}-\d{3}$", ErrorMessage = "Invalid ZIP code format (e.g., 12345-678).")]
        public string? AddressZipCode { get; set; }
    }
}