using System.ComponentModel.DataAnnotations;
using viaggia_server.Models.Payments;
using viaggia_server.Models.Reservations;
using viaggia_server.Models.UserRoles;
using viaggia_server.Repositories;

namespace viaggia_server.Models.Users
{
    public class User : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;
        public string? GoogleId { get; set; } // ID do Google (para autenticação via Google)
        
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(256, ErrorMessage = "Password cannot exceed 256 characters.")]
        public string Password { get; set; } = null!; // Encrypted password

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = null!; // Ex.: +5511999999999

        public string AvatarUrl { get; set; } = null!; // URL do avatar do usuário

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // For CLIENT
        [StringLength(14, ErrorMessage = "CPF must be 14 characters (e.g., 123.456.789-00).")]
        public string? Cpf { get; set; }

        // For SERVICE_PROVIDER
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters.")]
        public string? CompanyName { get; set; }

        [StringLength(18, ErrorMessage = "CNPJ must be 18 characters (e.g., 12.345.678/0001-99).")]
        public string? Cnpj { get; set; }

        [StringLength(100, ErrorMessage = "Company legal name cannot exceed 100 characters.")]
        public string? CompanyLegalName { get; set; }

        // For ATTENDANT
        [StringLength(100, ErrorMessage = "Employer company name cannot exceed 100 characters.")]
        public string? EmployerCompanyName { get; set; }

        [StringLength(50, ErrorMessage = "Employee ID cannot exceed 50 characters.")]
        public string? EmployeeId { get; set; }

        // Relationships
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}