using Microsoft.AspNetCore.Identity.Data;
using viaggia_server.Models.User;

namespace viaggia_server.Repositories.Auth
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string senha);
        Task<List<User>> GetAllUsersAsync();
        Task<User> RegisterAsync(RegisterRequest request);
    }

}
