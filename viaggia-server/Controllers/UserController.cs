using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Users;
using viaggia_server.Services.Users;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Cria um novo cliente.
        /// </summary>
        /// <param name="request">Dados do cliente.</param>
        /// <returns>Cliente criado.</returns>
        [HttpPost("client")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid client data.", ModelState));

            try
            {
                var result = await _service.CreateClientAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id },
                    new ApiResponse(true, "Client created successfully.", result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(false, $"Error creating client: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cria um novo prestador de serviços.
        /// </summary>
        /// <param name="request">Dados do prestador de serviços.</param>
        /// <returns>Prestador de serviços criado.</returns>
        [HttpPost("service-provider")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateServiceProvider([FromBody] CreateServiceProviderDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid service provider data.", ModelState));

            try
            {
                var result = await _service.CreateServiceProviderAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id },
                    new ApiResponse(true, "Service provider created successfully.", result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(false, $"Error creating service provider: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cria um novo atendente.
        /// </summary>
        /// <param name="request">Dados do atendente.</param>
        /// <returns>Atendente criado.</returns>
        [HttpPost("attendant")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAttendant([FromBody] CreateAttendantDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid attendant data.", ModelState));

            try
            {
                var result = await _service.CreateAttendantAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id },
                    new ApiResponse(true, "Attendant created successfully.", result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(false, $"Error creating attendant: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtém todos os usuários ativos.
        /// </summary>
        /// <returns>Lista de usuários ativos.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(new ApiResponse(true, "Users retrieved successfully.", result));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(false, $"Error retrieving users: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtém um usuário por ID.
        /// </summary>
        /// <param name="id">ID do usuário.</param>
        /// <returns>Dados do usuário.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse(false, "Invalid user ID."));

            try
            {
                var result = await _service.GetByIdAsync(id);
                return Ok(new ApiResponse(true, "User retrieved successfully.", result));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(false, $"Error retrieving user: {ex.Message}"));
            }
        }

        /// <summary>
        /// Exclui logicamente um usuário por ID.
        /// </summary>
        /// <param name="id">ID do usuário.</param>
        /// <returns>Confirmação da exclusão.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SoftDelete(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse(false, "Invalid user ID."));

            try
            {
                await _service.SoftDeleteAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(false, $"Error deleting user: {ex.Message}"));
            }
        }

        /// <summary>
        /// Reativa um usuário excluído logicamente por ID.
        /// </summary>
        /// <param name="id">ID do usuário.</param>
        /// <returns>Confirmação da reativação.</returns>
        [HttpPatch("{id}/reactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Reactivate(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse(false, "Invalid user ID."));

            try
            {
                await _service.ReactivateAsync(id);
                return Ok(new ApiResponse(true, "User reactivated successfully."));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(false, $"Error reactivating user: {ex.Message}"));
            }
        }
    }
}