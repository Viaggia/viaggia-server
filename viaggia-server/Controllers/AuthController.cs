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

        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequestDTO request)
        {
            try
            {
                var result = await _authService.ValidatePasswordResetTokenAsync(request.Token);
                
                if (!result.IsValid)
                {
                    return BadRequest(new { 
                        Message = result.Message,
                        IsValid = false 
                    });
                }

                return Ok(new { 
                    Message = result.Message,
                    IsValid = true,
                    UserName = result.UserName,
                    Email = result.Email,
                    ExpiryDate = result.ExpiryDate
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
        {
            try
            {
                // Primeiro valida o token
                var tokenValidation = await _authService.ValidatePasswordResetTokenAsync(request.Token);
                if (!tokenValidation.IsValid)
                {
                    return BadRequest(new { Message = tokenValidation.Message });
                }

                var success = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
                if (!success)
                    return BadRequest(new { Message = "Erro interno. Tente novamente." });

                return Ok(new { 
                    Message = "Senha redefinida com sucesso!",
                    UserName = tokenValidation.UserName
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
