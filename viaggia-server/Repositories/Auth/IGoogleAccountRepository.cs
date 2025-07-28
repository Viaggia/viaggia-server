using viaggia_server.DTOs.Auth;
using viaggia_server.Models.Users;

namespace viaggia_server.Repositories.Auth
{
    public interface IGoogleAccountRepository
    {
        // Cria um usuário via Gmail
        Task<User> CreateOrLoginOAuth(OAuthRequest dto);
    }
}
