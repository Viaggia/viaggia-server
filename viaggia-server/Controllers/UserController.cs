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
        private readonly ILogger<UsersController> _logger; 

        public UsersController(
            IUserRepository userRepository,
            IEmailService emailService,
            IValidator<CreateClientDTO> clientValidator,
            IValidator<CreateServiceProviderDTO> serviceProviderValidator,
            IValidator<CreateAttendantDTO> attendantValidator,
            IValidator<UpdateUserDTO> updateUserValidator,
            ILogger<UsersController> logger) 
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _clientValidator = clientValidator ?? throw new ArgumentNullException(nameof(clientValidator));
            _serviceProviderValidator = serviceProviderValidator ?? throw new ArgumentNullException(nameof(serviceProviderValidator));
            _attendantValidator = attendantValidator ?? throw new ArgumentNullException(nameof(attendantValidator));
            _updateUserValidator = updateUserValidator ?? throw new ArgumentNullException(nameof(updateUserValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                {
                    _logger.LogWarning("Validação falhou ao criar cliente: {Errors}", string.Join(", ", validationResult.Errors));
                    throw new FluentValidation.ValidationException(validationResult.Errors);
                }

                var result = await _userRepository.CreateClientAsync(request);
                try
                {
                    await _emailService.SendWelcomeEmailAsync(result.Email, result.Name);
                    _logger.LogInformation("E-mail de boas-vindas enviado para {Email}", result.Email);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning("Falha ao enviar e-mail de boas-vindas para {Email}: {Error}", result.Email, emailEx.Message);
                }

                _logger.LogInformation("Cliente criado com sucesso: UserId {Id}", result.Id);
                return CreatedAtAction(nameof(GetById), new { id = result.Id },
                    new ApiResponse<UserDTO>(true, "Client created successfully.", result));
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new ApiResponse<UserDTO>(false, "Validation failed.", null, ex.Errors));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Erro ao criar cliente: {Error}", ex.Message);
                return BadRequest(new ApiResponse<UserDTO>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao criar cliente.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<UserDTO>(false, $"Error creating client: {ex.Message}"));
            }
        }

        [HttpPost("service-provider")]
        [Authorize(Roles = "ADMIN")] // Restrict to ADMIN role
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Add for unauthorized access
        [ProducesResponseType(StatusCodes.Status403Forbidden)] // Add for forbidden access
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateServiceProvider([FromBody] CreateServiceProviderDTO request)
        {
            try
            {
                var validationResult = await _serviceProviderValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Validação falhou ao criar provedor de serviços: {Errors}", string.Join(", ", validationResult.Errors));
                    throw new FluentValidation.ValidationException(validationResult.Errors);
                }

                var result = await _userRepository.CreateServiceProviderAsync(request);
                _logger.LogInformation("Provedor de serviços criado com sucesso por Admin: UserId {Id}", result.Id);
                return CreatedAtAction(nameof(GetById), new { id = result.Id },
                    new ApiResponse<UserDTO>(true, "Service provider created successfully.", result));
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new ApiResponse<UserDTO>(false, "Validation failed.", null, ex.Errors));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Erro ao criar provedor de serviços: {Error}", ex.Message);
                return BadRequest(new ApiResponse<UserDTO>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao criar provedor de serviços.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<UserDTO>(false, $"Error creating service provider: {ex.Message}"));
            }
        }

        [HttpPost("attendant")]
        [Authorize(Roles = "ADMIN")] // Restrict to ADMIN role
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Add for unauthorized access
        [ProducesResponseType(StatusCodes.Status403Forbidden)] // Add for forbidden access
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAttendant([FromBody] CreateAttendantDTO request)
        {
            try
            {
                var validationResult = await _attendantValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Validação falhou ao criar atendente: {Errors}", string.Join(", ", validationResult.Errors));
                    throw new FluentValidation.ValidationException(validationResult.Errors);
                }

                var result = await _userRepository.CreateAttendantAsync(request);
                _logger.LogInformation("Atendente criado com sucesso por Admin: UserId {Id}", result.Id);
                return CreatedAtAction(nameof(GetById), new { id = result.Id },
                    new ApiResponse<UserDTO>(true, "Attendant created successfully.", result));
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new ApiResponse<UserDTO>(false, "Validation failed.", null, ex.Errors));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Erro ao criar atendente: {Error}", ex.Message);
                return BadRequest(new ApiResponse<UserDTO>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao criar atendente.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<UserDTO>(false, $"Error creating attendant: {ex.Message}"));
            }
        }

        [HttpPost("admin")]
        [Authorize(Roles = "ADMIN")] 
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDTO request)
        {
            try
            {
                var result = await _userRepository.CreateAdminAsync(request);
                _logger.LogInformation("Administrador criado com sucesso por Admin: UserId {Id}", result.Id);
                return CreatedAtAction(nameof(GetById), new { id = result.Id },
                    new ApiResponse<UserDTO>(true, "Admin user created successfully.", result));
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogWarning("Validação falhou ao criar administrador: {Errors}", string.Join(", ", ex.Errors));
                return BadRequest(new ApiResponse<UserDTO>(false, "Validation failed.", null, ex.Errors));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Erro ao criar administrador: {Error}", ex.Message);
                return BadRequest(new ApiResponse<UserDTO>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao criar administrador.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<UserDTO>(false, $"Error creating admin user: {ex.Message}"));
            }
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _userRepository.GetAllAsync();
                _logger.LogInformation("Lista de usuários recuperada com sucesso. Total: {Count}", result.Count);
                return Ok(new ApiResponse<List<UserDTO>>(true, "Users retrieved successfully.", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao recuperar lista de usuários.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<List<UserDTO>>(false, $"Error retrieving users: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN,CLIENT,SERVICE_PROVIDER,ATTENDANT")] 
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de recuperar usuário com ID inválido: {Id}", id);
                return BadRequest(new ApiResponse<UserDTO>(false, "Invalid user ID."));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("ADMIN") && (userIdClaim == null || userIdClaim != id.ToString()))
            {
                _logger.LogWarning("Usuário {UserId} tentou acessar perfil de outro usuário: {TargetId}", userIdClaim, id);
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse<UserDTO>(false, "You can only access your own profile unless you are an Admin."));
            }

            try
            {
                var result = await _userRepository.GetByIdAsync(id);
                _logger.LogInformation("Usuário recuperado com sucesso: UserId {Id}", id);
                return Ok(new ApiResponse<UserDTO>(true, "User retrieved successfully.", result));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Usuário não encontrado: UserId {Id}, Erro: {Error}", id, ex.Message);
                return NotFound(new ApiResponse<UserDTO>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao recuperar usuário: UserId {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<UserDTO>(false, $"Error retrieving user: {ex.Message}"));
            }
        }

        [HttpDelete("{id}/deactivate")]
        [Authorize(Roles = "ADMIN")] 
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SoftDelete(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de desativar usuário com ID inválido: {Id}", id);
                return BadRequest(new ApiResponse<UserDTO>(false, "Invalid user ID."));
            }

            try
            {
                var deleted = await _userRepository.SoftDeleteAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("Usuário não encontrado para desativação: UserId {Id}", id);
                    return NotFound(new ApiResponse<UserDTO>(false, "User not found."));
                }
                _logger.LogInformation("Usuário desativado com sucesso: UserId {Id}", id);
                return Ok(new ApiResponse<string>(true, $"Usuário com ID {id} foi desativado com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao desativar usuário: UserId {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>(false, $"Erro ao desativar o usuário: {ex.Message}"));
            }
        }

        [HttpPatch("{id}/reactivate")]
        [Authorize(Roles = "ADMIN")] 
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Reactivate(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de reativar usuário com ID inválido: {Id}", id);
                return BadRequest(new ApiResponse<UserDTO>(false, "Invalid user ID."));
            }

            try
            {
                var reactivated = await _userRepository.ReactivateAsync(id);
                if (!reactivated)
                {
                    _logger.LogWarning("Usuário não encontrado para reativação: UserId {Id}", id);
                    return NotFound(new ApiResponse<UserDTO>(false, "User not found."));
                }
                _logger.LogInformation("Usuário reativado com sucesso: UserId {Id}", id);
                return Ok(new ApiResponse<UserDTO>(true, "User reactivated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao reativar usuário: UserId {Id}", id);
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
            {
                _logger.LogWarning("Tentativa de atualizar usuário com ID inválido: {Id}", id);
                return BadRequest(new ApiResponse<UserDTO>(false, "ID de usuário inválido."));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("ADMIN") && (userIdClaim == null || userIdClaim != id.ToString()))
            {
                _logger.LogWarning("Usuário {UserId} tentou atualizar outro usuário: {TargetId}", userIdClaim, id);
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse<UserDTO>(false, "Você só pode editar o seu próprio perfil a menos que seja um Admin."));
            }

            try
            {
                var validationResult = await _updateUserValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Validação falhou ao atualizar usuário {Id}: {Errors}", id, string.Join(", ", validationResult.Errors));
                    throw new FluentValidation.ValidationException(validationResult.Errors);
                }

                var result = await _userRepository.UpdateAsync(id, request);
                _logger.LogInformation("Usuário atualizado com sucesso: UserId {Id}", id);
                return Ok(new ApiResponse<UserDTO>(true, "Usuário atualizado com sucesso.", result));
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new ApiResponse<UserDTO>(false, "Falha de validação.", null, ex.Errors));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Erro ao atualizar usuário {Id}: {Error}", id, ex.Message);
                return NotFound(new ApiResponse<UserDTO>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao atualizar usuário: UserId {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<UserDTO>(false, $"Erro ao atualizar usuário: {ex.Message}"));
            }
        }
    }
}