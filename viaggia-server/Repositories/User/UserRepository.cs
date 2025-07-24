using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Users;

namespace viaggia_server.Repositories.Users
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> CpfExistsAsync(string? cpf)
        {
            return cpf != null && await _context.Users.AnyAsync(u => u.Cpf == cpf);
        }

        public async Task<bool> CnpjExistsAsync(string? cnpj)
        {
            return cnpj != null && await _context.Users.AnyAsync(u => u.Cnpj == cnpj);
        }
    }
}