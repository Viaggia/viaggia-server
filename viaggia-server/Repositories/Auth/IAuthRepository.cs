using viaggia_server.Models.Users;

namespace viaggia_server.Repositories.Auth
{
    public interface IAuthRepository
    {
        Task<string> LoginAsync(string email, string password);
    }

}
