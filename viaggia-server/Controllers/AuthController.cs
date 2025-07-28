using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs.Auth;
using viaggia_server.DTOs.User;
using viaggia_server.Services.Auth;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            try
            {
                var token = await _authService.LoginAsync(request.Email, request.Password);
                // Buscar o usuário para retornar informações adicionais
                var user = await _authService.GetUserByEmailAsync(request.Email); // Método adicional necessário no serviço
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
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { Message = "Email ou senha incorretos." });
            }
        }


        [Authorize]
        [HttpPost("logout-default")]
        public async Task<IActionResult> Logout()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
                return BadRequest(new { Message = "Token não fornecido." });

            await _authService.RevokeTokenAsync(token);
            return Ok(new { Message = "Logout do JWT realizado com sucesso. Token revogado." });
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
        {
            try
            {
                await _authService.GeneratePasswordResetTokenAsync(request.Email);
                return Ok(new { Message = "E-mail de redefinição de senha enviado com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
        {
            var success = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
            if (!success)
                return BadRequest(new { Message = "Token inválido, expirado ou já utilizado." });

            return Ok(new { Message = "Senha redefinida com sucesso." });
        }
    }
}
