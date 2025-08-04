using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using viaggia_server.Data;
using viaggia_server.DTOs.Auth;
using viaggia_server.Models.Users;

namespace viaggia_server.Repositories.Auth
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthRepository> _logger;

        public AuthRepository(AppDbContext context, IConfiguration configuration, ILogger<AuthRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                throw new UnauthorizedAccessException("Email ou senha incorretos.");
            }
            return await GenerateJwtTokenAsync(user); // Ensure GenerateJwtTokenAsync is used
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }

        public async Task<string> GenerateJwtTokenAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email)
            };
            claims.AddRange(user.UserRoles.Select(ur => new Claim(ClaimTypes.Role, ur.Role.Name)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddHours(4);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task RevokeTokenAsync(string token)
        {
            _logger.LogInformation("Revoking token for user.");
            var revokedToken = new RevokedToken
            {
                Token = token,
                RevokedAt = DateTime.UtcNow
            };
            await _context.RevokedTokens.AddAsync(revokedToken);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsTokenRevokedAsync(string token)
        {
            return await _context.RevokedTokens.AnyAsync(rt => rt.Token == token);
        }

        //public async Task<string> GeneratePasswordResetTokenAsync(string email)
        //{
        //    var user = await GetUserByEmailAsync(email);
        //    if (user == null)
        //        throw new ArgumentException("Usuário não encontrado.");

        //    var token = Guid.NewGuid().ToString();
        //    var resetToken = new PasswordResetToken
        //    {
        //        UserId = user.Id,
        //        Token = token,
        //        ExpiryDate = DateTime.UtcNow.AddHours(1)
        //    };

        //    await _context.PasswordResetTokens.AddAsync(resetToken);
        //    await _context.SaveChangesAsync();
        //    return token;
        //}

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null)
                throw new ArgumentException("Usuário não encontrado.");

            // Generate a 6-digit numeric token
            var random = new Random();
            var token = random.Next(100000, 999999).ToString("D6"); // Ensures 6 digits

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddHours(1)
            };

            await _context.PasswordResetTokens.AddAsync(resetToken);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<ValidateTokenResponseDTO> ValidatePasswordResetTokenAsync(string token)
        {
            var resetToken = await _context.PasswordResetTokens
                .Include(prt => prt.User)
                .FirstOrDefaultAsync(prt => prt.Token == token);

            if (resetToken == null)
            {
                return new ValidateTokenResponseDTO
                {
                    IsValid = false,
                    Message = "Token inválido."
                };
            }

            if (resetToken.ExpiryDate < DateTime.UtcNow)
            {
                return new ValidateTokenResponseDTO
                {
                    IsValid = false,
                    Message = "Token expirado."
                };
            }

            return new ValidateTokenResponseDTO
            {
                IsValid = true,
                Message = "Token válido.",
                UserName = resetToken.User.Name,
                Email = resetToken.User.Email,
                ExpiryDate = resetToken.ExpiryDate
            };
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var resetToken = await _context.PasswordResetTokens
                .Include(prt => prt.User)
                .FirstOrDefaultAsync(prt => prt.Token == token);

            if (resetToken == null || resetToken.ExpiryDate < DateTime.UtcNow)
                return false;

            var user = resetToken.User;
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.PasswordResetTokens.Remove(resetToken);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}