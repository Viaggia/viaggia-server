using viaggia_server.DTOs.Auth;
using viaggia_server.Models.Users;

namespace viaggia_server.Repositories.Users
{
    public interface IUserRepository : IRepository<User>
    {
        // Reativa um usuário
        Task<bool> ReactivateAsync(int id);
        // Verifica se um email já existe
        Task<bool> EmailExistsAsync(string email);
        Task<bool> CpfExistsAsync(string? cpf);
    }
}