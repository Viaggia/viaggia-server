using viaggia_server.DTOs;
using viaggia_server.Models.User;

namespace viaggia_server.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<string> LoginAsync(string email, string senha);
        Task<List<User>> GetAllUsersAsync();
        Task<User> RegisterAsync(RegisterRequest request);
    }

}
