using Microsoft.AspNetCore.Identity.Data;

namespace viaggia_server.Repositories.Auth
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string senha);
        Task<List<Usuario>> GetAllUsersAsync();
        Task<Usuario> RegisterAsync(RegisterRequest request);
    }

}
