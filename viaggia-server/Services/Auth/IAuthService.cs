using viaggia_server.DTOs.Auth;
using viaggia_server.Models.Users;

namespace viaggia_server.Services.Auth
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string password);
        Task<string> GenerateJwtToken(User user);
        Task<User> GetUserByEmailAsync(string email);
    }

}