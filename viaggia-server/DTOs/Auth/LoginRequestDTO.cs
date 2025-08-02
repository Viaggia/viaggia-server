namespace viaggia_server.DTOs.Auth
{
    public class LoginRequestDTO
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}