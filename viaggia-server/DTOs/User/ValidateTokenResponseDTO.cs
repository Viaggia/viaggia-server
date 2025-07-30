namespace viaggia_server.DTOs.User
{
    public class ValidateTokenResponseDTO
    {
        public bool IsValid { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Message { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
