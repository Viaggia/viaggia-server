using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.User
{
    public class UpdateUserDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }

        // Optional fields based on user role
        [StringLength(14, ErrorMessage = "CPF must be 14 characters (e.g., 123.456.789-00).")]
        public string? Cpf { get; set; } // For Clients

        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters.")]
        public string? CompanyName { get; set; } // For Service Providers

        [StringLength(100, ErrorMessage = "Company legal name cannot exceed 100 characters.")]
        public string? CompanyLegalName { get; set; } // For Service Providers

        [StringLength(100, ErrorMessage = "Employer company name cannot exceed 100 characters.")]
        public string? EmployerCompanyName { get; set; } // For Attendants

        [StringLength(50, ErrorMessage = "Employee ID cannot exceed 50 characters.")]
        public string? EmployeeId { get; set; } // For Attendants

        // Image upload
        public IFormFile? Avatar { get; set; } // Optional image file
    }
}
