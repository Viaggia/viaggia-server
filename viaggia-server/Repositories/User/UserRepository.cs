using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.DTOs.Auth;
using viaggia_server.Models.Users;
using viaggia_server.Models.UserRoles;

namespace viaggia_server.Repositories.Users
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
            
        }

        public Task<User> CreateAsync(User user, string roleName)
        {
            // Verifica se a role existe
            var role = _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
            {
                throw new ArgumentException($"Role '{roleName}' does not exist.");
            }
            // Adiciona o usuário ao contexto
            _context.Users.Add(user);
            _context.SaveChanges();
            // Cria a relação entre o usuário e a role
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = role.Result.Id
            };
            _context.UserRoles.Add(userRole);
            _context.SaveChanges();
            return Task.FromResult(user);
        }

        public async Task<bool> ReactivateAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }
            user.IsActive = true;
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

        public async Task<User> CreateOrLoginOAuth(OAuthRequest dto)
        {
            // Tenta encontrar o usuário pelo Google ID
            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == dto.GoogleUid);
            if (user == null)
            {
                var generatedPassword = Guid.NewGuid().ToString(); // Gera uma senha padrão
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(generatedPassword); // Criptografa a senha

                // Se não encontrar, cria um novo usuário
                user = new User
                {
                    GoogleId = dto.GoogleUid,
                    Email = dto.Email,
                    Name = dto.Name,
                    AvatarUrl = dto.Picture,
                    PhoneNumber = dto.PhoneNumber ?? "default-password", // Defina uma senha padrão ou gere uma
                    Password = hashedPassword, // Senha não é necessária para OAuth
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
                user.AvatarUrl = dto.Picture;
            }
            await _context.SaveChangesAsync();
            return user;
        }
    }
}