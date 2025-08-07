using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using viaggia_server.Data;
using viaggia_server.DTOs.Auth;
using viaggia_server.Models.Users;
using viaggia_server.Repositories;
using viaggia_server.Repositories.Auth;

namespace viaggia_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IGoogleAccountRepository _repository;

        public AccountsController(IAuthRepository authRepository, IGoogleAccountRepository repository)
        {
            _authRepository = authRepository;
            _repository = repository;
        }

        [HttpGet("login-google")]
        public IActionResult LoginWithGoogle()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/api/Accounts/google-callback"
            };

            properties.Items["prompt"] = "select_account";

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return Unauthorized(new { Message = "Google authentication failed." });
            }

            var claims = result.Principal.Identities.First().Claims;
            var googleUid = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var picture = claims.FirstOrDefault(c => c.Type == "picture")?.Value;
            var phoneNumber = claims.FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone)?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { Message = "Google login did not return an email." });
            }

            var oauthRequest = new OAuthRequest
            {
                GoogleUid = googleUid,
                Email = email,
                Name = name,
                Picture = picture,
                PhoneNumber = phoneNumber
            };

            var user = await _repository.CreateOrLoginOAuth(oauthRequest);
            var token = await _authRepository.GenerateJwtTokenAsync(user);

            return Redirect($"http://localhost:5173/auth-success?token={token}");
        }

        //[HttpPost("complete-profile")]
        //[Authorize]
        //public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileDTO dto)
        //{
        //    try
        //    {
        //        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        //        {
        //            return Unauthorized(new { Message = "User not authenticated." });
        //        }

        //        var user = await _context.Users
        //            .Include(u => u.UserRoles)
        //            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        //        if (user == null)
        //        {
        //            return NotFound(new { Message = "User not found." });
        //        }

        //        // Update user details
        //        user.PhoneNumber = dto.PhoneNumber;

        //        // Assign role
        //        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role);
        //        if (role == null)
        //        {
        //            return BadRequest(new { Message = "Invalid role specified." });
        //        }

        //        user.UserRoles.Clear(); // Remove existing roles (e.g., default CLIENT)
        //        user.UserRoles.Add(new UserRole { RoleId = role.Id });

        //        await _context.SaveChangesAsync();

        //        // Generate new token with updated roles
        //        var token = await _authRepository.GenerateJwtTokenAsync(user);
        //        return Ok(new LoginResponseDTO
        //        {
        //            Token = token,
        //            Name = user.Name,
        //            Email = user.Email,
        //            PhoneNumber = user.PhoneNumber,
        //            Picture = user.AvatarUrl,
        //            NeedsProfileCompletion = false
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Message = $"Error completing profile: {ex.Message}" });
        //    }
        //}


        [HttpPost("logout-google")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { Message = "Logout successful" });
        }
    }
}