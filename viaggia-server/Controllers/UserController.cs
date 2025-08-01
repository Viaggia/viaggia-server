using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.User;
using viaggia_server.DTOs.Users;
using viaggia_server.Repositories.Users;
using viaggia_server.Services.EmailResetPassword;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IValidator<CreateClientDTO> _clientValidator;
        private readonly IValidator<CreateServiceProviderDTO> _serviceProviderValidator;
        private readonly IValidator<CreateAttendantDTO> _attendantValidator;
        private readonly IValidator<UpdateUserDTO> _updateUserValidator;

        public UsersController(
            IUserRepository userRepository,
            IEmailService emailService,
            IValidator<CreateClientDTO> clientValidator,
            IValidator<CreateServiceProviderDTO> serviceProviderValidator,
            IValidator<CreateAttendantDTO> attendantValidator,
            IValidator<UpdateUserDTO> updateUserValidator)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _clientValidator = clientValidator ?? throw new ArgumentNullException(nameof(clientValidator));
            _serviceProviderValidator = serviceProviderValidator ?? throw new ArgumentNullException(nameof(serviceProviderValidator));
            _attendantValidator = attendantValidator ?? throw new ArgumentNullException(nameof(attendantValidator));
            _updateUserValidator = updateUserValidator ?? throw new ArgumentNullException(nameof(updateUserValidator));
        }

        [HttpPost("client")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientDTO request)
        {
            try
            {
                var validationResult = await _clientValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var result = await _userRepository.CreateClientAsync(request);
                try
                {
                    await _emailService.SendWelcomeEmailAsync(result.Email, result.Name);
                }
                catch (Exception emailEx)
                {
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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateServiceProvider([FromBody] CreateServiceProviderDTO request)
        {
            try
            {
                var validationResult = await _serviceProviderValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var result = await _userRepository.CreateServiceProviderAsync(request);
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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAttendant([FromBody] CreateAttendantDTO request)
        {
            try
            {
                var validationResult = await _attendantValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var result = await _userRepository.CreateAttendantAsync(request);
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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDTO request)
        {
            try
            {
                var result = await _userRepository.CreateAdminAsync(request);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _userRepository.GetAllAsync();
                return Ok(new ApiResponse<List<UserDTO>>(true, "Users retrieved successfully.", result));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<List<UserDTO>>(false, $"Error retrieving users: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
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
                var result = await _userRepository.GetByIdAsync(id);
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
                var deleted = await _userRepository.SoftDeleteAsync(id);
                if (!deleted)
                    return NotFound(new ApiResponse<UserDTO>(false, "User not found."));
                return Ok(new ApiResponse<string>(true, $"Usuário com ID {id} foi desativado com sucesso."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>(false, $"Erro ao desativar o usuário: {ex.Message}"));
            }
        }

        [HttpPatch("{id}/reactivate")]
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
                var reactivated = await _userRepository.ReactivateAsync(id);
                if (!reactivated)
                    return NotFound(new ApiResponse<UserDTO>(false, "User not found."));
                return Ok(new ApiResponse<UserDTO>(true, "User reactivated successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<UserDTO>(false, $"Error reactivating user: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,CLIENT,SERVICE_PROVIDER,ATTENDANT")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateUserDTO request)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<UserDTO>(false, "ID de usuário inválido."));

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("ADMIN") && (userIdClaim == null || userIdClaim != id.ToString()))
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse<UserDTO>(false, "Você só pode editar o seu próprio perfil a menos que seja um Admin."));

            try
            {
                var validationResult = await _updateUserValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var result = await _userRepository.UpdateAsync(id, request);
                return Ok(new ApiResponse<UserDTO>(true, "Usuário atualizado com sucesso.", result));
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new ApiResponse<UserDTO>(false, "Falha de validação.", null, ex.Errors));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse<UserDTO>(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<UserDTO>(false, $"Erro ao atualizar usuário: {ex.Message}"));
            }
        }
    }
}