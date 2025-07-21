using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.User;
using viaggia_server.Repositories.Interfaces;

namespace viaggia_server.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> CreateAsync(User user, string roleName)
        {
            var role = await _context.Roles.SingleOrDefaultAsync(r => r.Name == roleName)
                ?? throw new Exception("Invalid role.");

            user.UserRoles.Add(new UserRole
            {
                User = user,
                Role = role
            });

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Usuarios
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Usuarios
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task DeleteAsync(User user)
        {
            _context.Usuarios.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
