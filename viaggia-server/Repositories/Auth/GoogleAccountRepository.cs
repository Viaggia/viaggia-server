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
        private readonly ILogger<GoogleAccountRepository> _logger; // Add logger

        public GoogleAccountRepository(AppDbContext context, ILogger<GoogleAccountRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<User> CreateOrLoginOAuth(OAuthRequest dto)
        {
            _logger.LogInformation("Processing OAuth request for GoogleUid: {GoogleUid}, Email: {Email}", dto.GoogleUid, dto.Email);

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.GoogleId == dto.GoogleUid);

            if (user == null)
            {
                var clientRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "CLIENT");
                if (clientRole == null)
                {
                    _logger.LogError("CLIENT role not found in database.");
                    throw new InvalidOperationException("CLIENT role not found in database.");
                }

                user = new User
                {
                    GoogleId = dto.GoogleUid,
                    Email = dto.Email,
                    Name = dto.Name,
                    AvatarUrl = dto.Picture ?? string.Empty,
                    PhoneNumber = dto.PhoneNumber,
                    Password = null,
                    IsActive = true,
                    CreateDate = DateTime.UtcNow,
                    UserRoles = new List<UserRole>
                    {
                        new UserRole
                        {
                            RoleId = clientRole.Id,
                            Role = clientRole
                        }
                    }
                };
                await _context.Users.AddAsync(user);
                _logger.LogInformation("Created new user with ID: {UserId}, Role: CLIENT", user.Id);
            }
            else
            {
                user.Email = dto.Email;
                user.Name = dto.Name;
                user.AvatarUrl = dto.Picture ?? string.Empty;
                _logger.LogInformation("Updated existing user with ID: {UserId}", user.Id);
            }

            await _context.SaveChangesAsync();

            // Log roles for debugging
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            _logger.LogInformation("User {UserId} has roles: {Roles}", user.Id, string.Join(", ", roles));

            return user;
        }
    }
}