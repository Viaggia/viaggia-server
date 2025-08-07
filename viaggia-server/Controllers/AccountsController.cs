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

            var frontendUrl = "http://localhost:5173/auth-success"; 
            var redirectUrl = $"{frontendUrl}?token={Uri.EscapeDataString(token)}&name={Uri.EscapeDataString(user.Name)}&email={Uri.EscapeDataString(user.Email)}&needsProfileCompletion={string.IsNullOrEmpty(user.PhoneNumber)}";

            return Redirect(redirectUrl);
        }

        [HttpPost("logout-google")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { Message = "Logout successful" });
        }


    }
}