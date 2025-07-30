using viaggia_server.DTOs.User;
using viaggia_server.Models.Users;

namespace viaggia_server.Repositories.Auth
{
    public interface IAuthRepository
    {
        Task<string> LoginAsync(string email, string password);
        Task<User> GetUserByEmailAsync(string email);
        Task<string> GenerateJwtTokenAsync(User user);
        Task RevokeTokenAsync(string token);
        Task<bool> IsTokenRevokedAsync(string token);
        Task<string> GeneratePasswordResetTokenAsync(string email);
        Task<ValidateTokenResponseDTO> ValidatePasswordResetTokenAsync(string token);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }

}
