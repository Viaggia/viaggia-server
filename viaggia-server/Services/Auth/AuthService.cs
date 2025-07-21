using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using viaggia_server.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using viaggia_server.Data;
using viaggia_server.Models.User;
using viaggia_server.Repositories.Interfaces;

namespace viaggia_server.Services.Auth
{
    public class AuthService : IAuthRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> LoginAsync(string email, string senha)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(u => u.Email == email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(senha, usuario.Password))
                throw new UnauthorizedAccessException("Credenciais inválidas");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Name),
                new Claim(ClaimTypes.Email, usuario.Email)
            };

            foreach (var role in usuario.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role.Name));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<User> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == request.Email))
                throw new Exception("Email já cadastrado.");

            var usuario = new User
            {
                Name = request.Name,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreateDate = DateTime.UtcNow
            };

            // Pega role pelo nome
            var role = await _context.Roles.SingleOrDefaultAsync(r => r.Name == request.RoleNome);
            if (role == null)
                throw new Exception("Role inválida.");

            var usuarioRole = new UserRole
            {
                User = usuario,
                Role = role
            };

            usuario.UserRoles.Add(usuarioRole);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return usuario;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Usuarios
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .ToListAsync();
        }
    }
}
