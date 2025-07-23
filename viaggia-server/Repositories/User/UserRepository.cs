using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.UserRoles;
using viaggia_server.Models.Users;

namespace viaggia_server.Repositories.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<User> CreateAsync(User user, string roleName)
        {
            // Adiciona o usuário ao banco
            await _context.Users.AddAsync(user);

            // Busca a role pelo nome
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName)
                ?? throw new Exception($"Role {roleName} not found.");

            // Cria o relacionamento UserRole
            var userRole = new UserRole { User = user, Role = role };
            await _context.UserRoles.AddAsync(userRole);

            return user;
        }

        public async Task<bool> ReactivateAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            user.IsActive = true;
            _context.Users.Update(user);
            return true;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> CpfExistsAsync(string cpf)
        {
            return await _context.Users.AnyAsync(u => u.Cpf == cpf);
        }

        public async Task<bool> CnpjExistsAsync(string cnpj)
        {
            return await _context.Users.AnyAsync(u => u.Cnpj == cnpj);
        }
    }
}