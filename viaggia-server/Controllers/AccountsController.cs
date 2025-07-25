using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using viaggia_server.Data;
using viaggia_server.DTOs.Auth;
using viaggia_server.Repositories.Users;

namespace viaggia_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IUserRepository _repository;

        public AccountsController(IUserRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("login-google")]
        public IActionResult LoginWithGoogle()
        {
            var properties = new AuthenticationProperties 
            {
                RedirectUri = "/api/Accounts/google-callback"
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded) return Unauthorized();

            var claims = result.Principal.Identities.First().Claims;
            var googleUid = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var picture = claims.FirstOrDefault(c => c.Type == "picture")?.Value;
            var password = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var phoneNumber = claims.FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone)?.Value;

            // Validação básica
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("O login do Google não retornou um e-mail.");

            var user = await _repository.CreateOrLoginOAuth(googleUid, email, name, picture,password, phoneNumber);

            return Ok(new
            {
                Name = user.Name,
                Email = user.Email,
                Password = user.Password,
                phoneNumber = user.PhoneNumber,
                Picture = user.AvatarUrl
            });
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { Message = "Logout successful" });
        }
    }
}
