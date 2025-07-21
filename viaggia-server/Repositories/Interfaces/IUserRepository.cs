using viaggia_server.Models.User;

namespace viaggia_server.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> CreateAsync(User user, string roleName);
        Task<List<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task DeleteAsync(User user);
        Task SaveAsync();
    }
}
