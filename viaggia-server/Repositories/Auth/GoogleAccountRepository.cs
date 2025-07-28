using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.DTOs.Auth;
using viaggia_server.Models.Users;

namespace viaggia_server.Repositories.Auth
{
    public class GoogleAccountRepository : IGoogleAccountRepository
    {
        private readonly AppDbContext _context;

        public GoogleAccountRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<User> CreateOrLoginOAuth(OAuthRequest dto)
        {
            // Tenta encontrar o usuário pelo GoogleId
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.GoogleId == dto.GoogleUid);

            if (user == null)
            {
                // Cria um novo usuário
                user = new User
                {
                    GoogleId = dto.GoogleUid,
                    Email = dto.Email,
                    Name = dto.Name,
                    AvatarUrl = dto.Picture ?? string.Empty,
                    PhoneNumber = dto.PhoneNumber, // Nullable, no default value
                    Password = null, // No password for OAuth users
                    IsActive = true,
                    CreateDate = DateTime.UtcNow
                };
                await _context.Users.AddAsync(user);
            }
            else
            {
                // Atualiza os dados do usuário existente
                user.Email = dto.Email;
                user.Name = dto.Name;
                user.AvatarUrl = dto.Picture ?? string.Empty;
            }

            await _context.SaveChangesAsync();
            return user;
        }
    }
}