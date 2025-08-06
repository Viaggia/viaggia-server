using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.DTOs.Auth;
using viaggia_server.Models.Users;
using viaggia_server.Repositories.Users;

namespace viaggia_server.Repositories.Auth
{
    public class GoogleAccountRepository : IGoogleAccountRepository
    {
        private readonly AppDbContext _context;
        private readonly IUserRepository _userRepository;

        public GoogleAccountRepository(AppDbContext context, IUserRepository userRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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
                user = await _userRepository.CreateWithRoleAsync(user, "CLIENT");
            }
            else
            {
                // Atualiza os dados do usuário existente
                user.Email = dto.Email;
                user.Name = dto.Name;
                user.AvatarUrl = dto.Picture ?? string.Empty;

                if (!user.UserRoles.Any(ur => ur.Role.Name == "CLIENT"))
                {
                    var clientRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "CLIENT");
                    if (clientRole != null)
                    {
                        await _context.UserRoles.AddAsync(new UserRole
                        {
                            UserId = user.Id,
                            RoleId = clientRole.Id
                        });
                        await _context.SaveChangesAsync();
                    }
                }
            }

            await _context.SaveChangesAsync();
            return user;
        }
    }
}