using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.DTOs.Auth;
using viaggia_server.Models.UserRoles;
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

        public async Task<bool> ReactivateAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false; // Usuário não encontrado
            }
            user.IsActive = true; // Reativa o usuário
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> CpfExistsAsync(string? cpf)
        {
            return cpf != null && await _context.Users.AnyAsync(u => u.Cpf == cpf);
        }

    }
}