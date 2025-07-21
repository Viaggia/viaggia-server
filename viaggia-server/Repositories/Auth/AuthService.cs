using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;

namespace viaggia_server.Repositories.Auth
{
    public class AuthService : IAuthService
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

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(Password, usuario.Senha))
                throw new UnauthorizedAccessException("Credenciais inválidas");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email)
            };

            foreach (var role in usuario.UsuarioRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role.Nome));
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

        public async Task<Usuario> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == request.Email))
                throw new Exception("Email já cadastrado.");

            var usuario = new Usuario
            {
                Nome = request.Nome,
                Email = request.Email,
                Telefone = request.Telefone,
                Senha = BCrypt.Net.BCrypt.HashPassword(request.Senha),
                CriadoEm = DateTime.UtcNow
            };

            // Pega role pelo nome
            var role = await _context.Roles.SingleOrDefaultAsync(r => r.Nome == request.RoleNome);
            if (role == null)
                throw new Exception("Role inválida.");

            var usuarioRole = new UsuarioRole
            {
                Usuario = usuario,
                Role = role
            };

            usuario.UsuarioRoles.Add(usuarioRole);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return usuario;
        }

        public async Task<List<Usuario>> GetAllUsersAsync()
        {
            return await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Role)
                .ToListAsync();
        }


    }
}
