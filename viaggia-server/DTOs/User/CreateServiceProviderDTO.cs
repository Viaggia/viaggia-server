using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Users
{
    public class CreateServiceProviderDTO
    {
        [Required(ErrorMessage = "Responsible name is required.")]
        [StringLength(100, ErrorMessage = "Responsible name cannot exceed 100 characters.")]
        public string ResponsibleName { get; set; } = null!;

        [Required(ErrorMessage = "Company name is required.")]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters.")]
        public string CompanyName { get; set; } = null!;

        [Required(ErrorMessage = "CNPJ is required.")]
        [RegularExpression(@"^\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}$", ErrorMessage = "Invalid CNPJ format (e.g., 12.345.678/0001-99).")]
        public string Cnpj { get; set; } = null!;

        [Required(ErrorMessage = "Legal name is required.")]
        [StringLength(200, ErrorMessage = "Legal name cannot exceed 200 characters.")]
        public string CompanyLegalName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\+?\d{10,15}$", ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
        public string Password { get; set; } = null!;
    }
}