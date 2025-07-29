using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Users;
using viaggia_server.Services.Users;
using viaggia_server.Services;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly IEmailService _emailService;

        public UsersController(IUserService service, IEmailService emailService)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        [HttpPost("client")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientDTO request)
        {
            try
            {
                var result = await _service.CreateClientAsync(request);
                
                // Enviar e-mail de boas-vindas
                try
                {
                    await _emailService.SendWelcomeEmailAsync(result.Email, result.Name);
                }
                catch (Exception emailEx)
                {
                    // Log do erro do e-mail, mas não falha a criação do usuário
                    // O usuário foi criado com sucesso, apenas o e-mail falhou
                    Console.WriteLine($"Falha ao enviar e-mail de boas-vindas: {emailEx.Message}");
                }
                
                return CreatedAtAction(nameof(GetById), new { id = result.Id },
                    new ApiResponse<UserDTO>(true, "Client created successfully.", result));
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new ApiResponse<UserDTO>(false, "Validation failed.", null, ex.Errors));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<UserDTO>(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<UserDTO>(false, $"Error creating client: {ex.Message}"));
            }
        }

        [HttpPost("service-provider")]
        //[Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateServiceProvider([FromBody] CreateServiceProviderDTO request)
        {
            try
            {
                var result = await _service.CreateServiceProviderAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id },
                    new ApiResponse<UserDTO>(true, "Service provider created successfully.", result));
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new ApiResponse<UserDTO>(false, "Validation failed.", null, ex.Errors));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<UserDTO>(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<UserDTO>(false, $"Error creating service provider: {ex.Message}"));
            }
        }

        [HttpPost("attendant")]
        //[Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAttendant([FromBody] CreateAttendantDTO request)
        {
            try
            {
                var result = await _service.CreateAttendantAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id },
                    new ApiResponse<UserDTO>(true, "Attendant created successfully.", result));
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new ApiResponse<UserDTO>(false, "Validation failed.", null, ex.Errors));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<UserDTO>(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<UserDTO>(false, $"Error creating attendant: {ex.Message}"));
            }
        }

        [HttpPost("admin")]
        //[Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDTO request)
        {
            try
            {
                var result = await _service.CreateAdminAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id },
                    new ApiResponse<UserDTO>(true, "Admin user created successfully.", result));
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new ApiResponse<UserDTO>(false, "Validation failed.", null, ex.Errors));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<UserDTO>(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<UserDTO>(false, $"Error creating admin user: {ex.Message}"));
            }
        }


        [HttpGet]
        //[Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(new ApiResponse<List<UserDTO>>(true, "Users retrieved successfully.", result));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<List<UserDTO>>(false, $"Error retrieving users: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        //[Authorize(Roles = "ADMIN,CLIENT,SERVICE_PROVIDER,ATTENDANT")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<UserDTO>(false, "Invalid user ID."));

            try
            {
                var result = await _service.GetByIdAsync(id);
                return Ok(new ApiResponse<UserDTO>(true, "User retrieved successfully.", result));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse<UserDTO>(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<UserDTO>(false, $"Error retrieving user: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        //[Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SoftDelete(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<UserDTO>(false, "Invalid user ID."));

            try
            {
                await _service.SoftDeleteAsync(id);

                return Ok(new ApiResponse<string>(
                    true,
                    $"Usuário com ID {id} foi desativado com sucesso."
                ));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse<string>(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>(false, $"Erro ao desativar o usuário: {ex.Message}"));
            }
        }


        [HttpPatch("{id}/reactivate")]
        //[Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Reactivate(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<UserDTO>(false, "Invalid user ID."));

            try
            {
                await _service.ReactivateAsync(id);
                return Ok(new ApiResponse<UserDTO>(true, "User reactivated successfully."));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse<UserDTO>(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<UserDTO>(false, $"Error reactivating user: {ex.Message}"));
            }
        }
    }
}