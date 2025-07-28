namespace viaggia_server.DTOs.User
{
    public class ResetPasswordRequestDTO
    {
        public string Token { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
