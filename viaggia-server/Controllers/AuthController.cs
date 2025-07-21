using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Auth;
using viaggia_server.DTOs.User;
using viaggia_server.Repositories.Interfaces;
using viaggia_server.Services.Auth;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var token = await _authService.LoginAsync(request.Email, request.Password);
                return Ok(new { Token = token });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { Message = "Email ou senha incorretos." });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var usuario = await _authService.RegisterAsync(request);
                return Ok(new
                {
                    usuario.Id,
                    usuario.Name,
                    usuario.Email,
                    usuario.PhoneNumber,
                    Roles = usuario.UserRoles.Select(r => r.Role.Name).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("usuarios")]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _authService.GetAllUsersAsync();

            var response = usuarios.Select(u => new UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
            }).ToList();

            return Ok(response);
        }
    }

}