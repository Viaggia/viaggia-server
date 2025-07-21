namespace viaggia_server.DTOs.User
{
    public class CreateServiceProviderRequest
    {
        public string CompanyName { get; set; } = null!;
        public string ResponsibleName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }
}
