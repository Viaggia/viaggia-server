using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using viaggia_server.Data;

namespace viaggia_server.Services.Auth
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
            // Validações de entrada
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email não pode ser vazio", nameof(email));
            
            if (string.IsNullOrWhiteSpace(senha))
                throw new ArgumentException("Senha não pode ser vazia", nameof(senha));

            var usuario = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(u => u.Email == email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(senha, usuario.Password))
                throw new UnauthorizedAccessException("Credenciais inválidas");

            // Verificar se o JWT Key está configurado
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT Key não está configurada no appsettings.json");

            var jwtIssuer = _configuration["Jwt:Issuer"];
            if (string.IsNullOrEmpty(jwtIssuer))
                throw new InvalidOperationException("JWT Issuer não está configurado no appsettings.json");

            var jwtAudience = _configuration["Jwt:Audience"];
            if (string.IsNullOrEmpty(jwtAudience))
                throw new InvalidOperationException("JWT Audience não está configurado no appsettings.json");

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

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
