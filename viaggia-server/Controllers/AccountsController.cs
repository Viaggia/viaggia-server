using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using viaggia_server.Data;
using viaggia_server.DTOs.Auth;
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
            var phoneNumber = claims.FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone)?.Value;

            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("O login do Google não retornou um e-mail.");

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
            
            //var frontendUrl = "http://localhost:5173/auth-success"; // Replace with your frontend URL
            //var redirectUrl = $"{frontendUrl}?token={Uri.EscapeDataString(token)}&name={Uri.EscapeDataString(user.Name)}&email={Uri.EscapeDataString(user.Email)}&needsProfileCompletion={string.IsNullOrEmpty(user.PhoneNumber)}";

            //return Redirect(redirectUrl);
            //https://localhost:7164/auth-sucess?token={token}
            return Redirect(  $"http://localhost:5174/auth-sucess?token={token}"); 
        }

        [HttpPost("logout-google")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { Message = "Logout successful" });
        }
    }
}