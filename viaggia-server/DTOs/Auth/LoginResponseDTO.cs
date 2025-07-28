namespace viaggia_server.DTOs.Auth
{
    public class LoginResponseDTO
    {
        public string Token { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Picture { get; set; }
        public bool IsNewUser { get; set; }
        public bool NeedsProfileCompletion { get; set; }
    }
}
