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
using viaggia_server.Services.Auth;

namespace viaggia_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IGoogleAccountRepository _repository;

        public AccountsController(IAuthService authService, IGoogleAccountRepository repository)
        {
            _authService = authService;
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
            var token = await _authService.GenerateJwtToken(user);

            return Ok(new LoginResponseDTO
            {
                Token = token,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Picture = user.AvatarUrl,
                NeedsProfileCompletion = string.IsNullOrEmpty(user.PhoneNumber)
            });
        }

        [HttpPost("logout-google")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { Message = "Logout successful" });
        }
    }
}