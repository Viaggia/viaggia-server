namespace viaggia_server.DTOs.Users
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Cpf { get; set; }
        public string? AddressStreet { get; set; }
        public string? AddressCity { get; set; }
        public string? AddressState { get; set; }
        public string? AddressZipCode { get; set; }
        public string? CompanyName { get; set; }
        public string? Cnpj { get; set; }
        public string? CompanyLegalName { get; set; }
        public string? EmployerCompanyName { get; set; }
        public string? EmployeeId { get; set; }
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}