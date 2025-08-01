using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// Endpoint público para testar se a API está funcionando
        /// </summary>
        [HttpGet("public")]
        public IActionResult TestPublic()
        {
            return Ok(new { message = "API está funcionando! Endpoint público." });
        }

        /// <summary>
        /// Endpoint autenticado para testar JWT
        /// </summary>
        [HttpGet("authenticated")]
        [Authorize]
        public IActionResult TestAuthenticated()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            return Ok(new 
            { 
                message = "Autenticação funcionando!",
                userId = userId,
                userName = userName,
                userEmail = userEmail,
                roles = roles,
                allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }

        /// <summary>
        /// Endpoint para debug do token JWT
        /// </summary>
        [HttpGet("debug-token")]
        public IActionResult DebugToken()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(authHeader))
            {
                return Ok(new { 
                    error = "Nenhum header Authorization encontrado",
                    headers = Request.Headers.Select(h => new { h.Key, h.Value }).ToList()
                });
            }

            if (!authHeader.StartsWith("Bearer "))
            {
                return Ok(new {
                    error = "Header Authorization não começa com 'Bearer '",
                    authHeader = authHeader,
                    length = authHeader.Length
                });
            }

            var token = authHeader.Substring(7); // Remove "Bearer "
            
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                
                return Ok(new {
                    message = "Token JWT válido (estruturalmente)",
                    header = jsonToken.Header,
                    payload = jsonToken.Payload,
                    claims = jsonToken.Claims.Select(c => new { c.Type, c.Value }).ToList(),
                    expires = jsonToken.ValidTo,
                    issued = jsonToken.ValidFrom
                });
            }
            catch (Exception ex)
            {
                return Ok(new {
                    error = "Token JWT inválido",
                    exception = ex.Message,
                    tokenStart = token.Substring(0, Math.Min(50, token.Length))
                });
            }
        }
    }
}
