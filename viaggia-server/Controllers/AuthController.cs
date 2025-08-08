using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs.Auth;
using viaggia_server.Repositories.Auth;
using viaggia_server.Services.Email;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IEmailService _emailService;


        public AuthController(IAuthRepository authRepository, IEmailService emailService)
        {
            _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            try
            {
                var token = await _authRepository.LoginAsync(request.Email, request.Password);
                var user = await _authRepository.GetUserByEmailAsync(request.Email);
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

            await _authRepository.RevokeTokenAsync(token);
            return Ok(new { Message = "Logout do JWT realizado com sucesso. Token revogado." });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
        {
            try
            {
                var user = await _authRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                    return BadRequest(new { Message = "Usuário não encontrado." });

                var token = await _authRepository.GeneratePasswordResetTokenAsync(request.Email);
                await _emailService.SendPasswordResetEmailAsync(request.Email, user.Name, token);
                return Ok(new { Message = "E-mail de redefinição de senha enviado com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Erro ao processar a solicitação: {ex.Message}" });
            }
        }

        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequestDTO request)
        {
            try
            {
                var result = await _authRepository.ValidatePasswordResetTokenAsync(request.Token);
                if (!result.IsValid)
                {
                    return BadRequest(new
                    {
                        Message = result.Message,
                        IsValid = false
                    });
                }

                return Ok(new
                {
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
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
        {
            try
            {
                var tokenValidation = await _authRepository.ValidatePasswordResetTokenAsync(request.Token);
                if (!tokenValidation.IsValid)
                {
                    return BadRequest(new { Message = tokenValidation.Message });
                }

                var success = await _authRepository.ResetPasswordAsync(request.Token, request.NewPassword);
                if (!success)
                    return BadRequest(new { Message = "Erro interno. Tente novamente." });

                return Ok(new
                {
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