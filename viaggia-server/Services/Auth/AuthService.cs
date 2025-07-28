using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using viaggia_server.Data;
using viaggia_server.Models;
using viaggia_server.Models.PasswordResetToken;
using viaggia_server.Models.RevokedToken;
using viaggia_server.Models.Users;
using viaggia_server.Repositories.Auth;
using viaggia_server.Repositories.Users;

namespace viaggia_server.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(
            IAuthRepository authRepository,
            IUserRepository userRepository,
            AppDbContext context,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _authRepository = authRepository;
            _userRepository = userRepository;
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            return await _authRepository.LoginAsync(email, password);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
                throw new Exception("Usuário não encontrado");
            return user;
        }

        public async Task<string> GenerateJwtToken(User user)
        {
            var userWithRoles = await _userRepository.GetByIdAsync(user.Id);
            if (userWithRoles == null)
                throw new Exception("Usuário não encontrado");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userWithRoles.Id.ToString()),
                new Claim(ClaimTypes.Name, userWithRoles.Name),
                new Claim(ClaimTypes.Email, userWithRoles.Email)
            };

            foreach (var role in userWithRoles.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role.Name));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task RevokeTokenAsync(string token)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var expiryDate = jwtToken.ValidTo;

            var revokedToken = new RevokedToken
            {
                Token = token,
                RevokedAt = DateTime.UtcNow,
                ExpiryDate = expiryDate
            };

            await _context.RevokedTokens.AddAsync(revokedToken);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsTokenRevokedAsync(string token)
        {
            return await _context.RevokedTokens.AnyAsync(rt => rt.Token == token && (rt.ExpiryDate == null || rt.ExpiryDate > DateTime.UtcNow));
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null || string.IsNullOrEmpty(user.Password))
                throw new Exception("Usuário não encontrado ou não possui senha definida (usuário OAuth).");

            // Generate a unique token
            var token = Guid.NewGuid().ToString();

            var resetToken = new PasswordResetToken
            {
                Token = token,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddHours(1), // Token valid for 1 hour
                IsUsed = false
            };

            await _context.PasswordResetTokens.AddAsync(resetToken);
            await _context.SaveChangesAsync();

            // Send email with reset link
            await _emailService.SendPasswordResetEmailAsync(email, token);

            return token;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var resetToken = await _context.PasswordResetTokens
                .Include(prt => prt.User)
                .FirstOrDefaultAsync(prt => prt.Token == token && !prt.IsUsed && prt.ExpiryDate > DateTime.UtcNow);

            if (resetToken == null)
                return false;

            resetToken.User.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            resetToken.IsUsed = true;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}