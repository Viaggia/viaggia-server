using viaggia_server.Models.Users;

namespace viaggia_server.Repositories.Users
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> EmailExistsAsync(string email);
        Task<bool> CpfExistsAsync(string? cpf);
        Task<bool> CnpjExistsAsync(string? cnpj);
    }
}