using System.ComponentModel.DataAnnotations;
using viaggia_server.Models.Payments;
using viaggia_server.Models.Reservations;
using viaggia_server.Models.UserRoles;
using viaggia_server.Models.Users;
using viaggia_server.Repositories;

namespace viaggia_server.Models.Users
{
    public class User : ISoftDeletable
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!; // Nome do usuário (Cliente, Prestador, Atendente)

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!; // Senha criptografada

        [Required]
        public string PhoneNumber { get; set; } = null!; // Telefone (ex.: +5511999999999)

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Para CLIENT
        public string? Cpf { get; set; } // CPF (ex.: 123.456.789-00)
        public string? AddressStreet { get; set; } // Rua
        public string? AddressCity { get; set; } // Cidade
        public string? AddressState { get; set; } // Estado (ex.: SP)
        public string? AddressZipCode { get; set; } // CEP (ex.: 12345-678)

        // Para SERVICE_PROVIDER
        public string? CompanyName { get; set; } // Nome da empresa (ex.: Viagens LTDA)
        public string? Cnpj { get; set; } // CNPJ (ex.: 12.345.678/0001-99)
        public string? CompanyLegalName { get; set; } // Razão social

        // Para ATTENDANT
        public string? EmployerCompanyName { get; set; } // Nome da empresa onde trabalha
        public string? EmployeeId { get; set; } // Número de identificação do funcionário (ex.: matrícula)

        // Relacionamentos
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}