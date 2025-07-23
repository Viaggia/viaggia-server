using viaggia_server.Models.Users;

namespace viaggia_server.Repositories.Users
{
    public interface IUserRepository
    {
        // Cria um usuário com uma role específica
        Task<User> CreateAsync(User user, string roleName);
        // Reativa um usuário
        Task<bool> ReactivateAsync(int id);
        // Verifica se um email já existe
        Task<bool> EmailExistsAsync(string email);
        // Verifica se um CPF já existe
        Task<bool> CpfExistsAsync(string cpf);
        // Verifica se um CNPJ já existe
        Task<bool> CnpjExistsAsync(string cnpj);
    }
}