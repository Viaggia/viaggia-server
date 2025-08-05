using viaggia_server.Models.Users;

namespace viaggia_server.DTOs.Users
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Cpf { get; set; }
        public string? CompanyName { get; set; }
        public string? Cnpj { get; set; }
        public string? CompanyLegalName { get; set; }
        public string? EmployerCompanyName { get; set; }
        public string? EmployeeId { get; set; }
        public bool IsActive { get; set; }
        public string? AvatarUrl { get; set; } = null!;
        public List<string> Roles { get; set; } = new List<string>();
        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();



    }
}