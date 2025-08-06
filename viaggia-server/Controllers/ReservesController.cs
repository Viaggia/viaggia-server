using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Reserves;
using viaggia_server.Services.Reserves;

namespace viaggia_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservesController : ControllerBase
    {
        private readonly IReservesService _reservesService;

        public ReservesController(IReservesService reservationService)
        {
            _reservesService = reservationService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReservations()
        {
            var result = await _reservesService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservationById(int id)
        {
            var result = await _reservesService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReservationsByUserId(int userId)
        {
            var result = await _reservesService.GetByUserIdAsync(userId);
            if (result == null || !result.Any()) return NotFound();
            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] ReserveCreateDTO dto)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _reservesService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetReservationById), new { id = result.ReserveId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReserveUpdateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _reservesService.UpdateAsync(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteReservation(int id)
        {
            var deleted = await _reservesService.SoftDeleteAsync(id);
            if(!deleted) return NotFound();
            return Ok(deleted);
        }
    }
}
