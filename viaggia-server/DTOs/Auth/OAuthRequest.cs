namespace viaggia_server.DTOs.Auth
{
    public class OAuthRequest
    {
        public string GoogleUid { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Picture { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
