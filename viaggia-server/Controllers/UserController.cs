using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs.User;
using viaggia_server.Services.Interfaces;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [HttpPost("client")]
        public async Task<IActionResult> CreateClient(CreateClientRequest request)
        {
            var result = await _service.CreateClientAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPost("service-provider")]
        public async Task<IActionResult> CreateServiceProvider(CreateServiceProviderRequest request)
        {
            var result = await _service.CreateServiceProviderAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPost("attendant")]
        public async Task<IActionResult> CreateAttendant(CreateAttendantRequest request)
        {
            var result = await _service.CreateAttendantAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
