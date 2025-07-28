using viaggia_server.DTOs.Auth;
using viaggia_server.Models.Users;

namespace viaggia_server.Services.Auth
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string password);
        Task<User> GetUserByEmailAsync(string email);
        Task<string> GenerateJwtToken(User user);
        Task RevokeTokenAsync(string token);
        Task<bool> IsTokenRevokedAsync(string token);
        Task<string> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }

}