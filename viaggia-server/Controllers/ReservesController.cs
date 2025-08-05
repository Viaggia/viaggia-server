using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Reserve;
using viaggia_server.Services.Reservations;

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
            try
            {
                var result = await _reservesService.GetAllAsync();
                return Ok(new ApiResponse<List<ReserveDTO>>(true, "Reservations retrieved successfully.", result));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>(false, $"Error retrieving reservations: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReservationById(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<string>(false, "Invalid reservation ID."));

            try
            {
                var result = await _reservesService.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new ApiResponse<string>(false, $"Reservation with ID {id} not found."));

                return Ok(new ApiResponse<ReserveDTO>(true, "Reservation retrieved successfully.", result));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>(false, $"Error retrieving reservation: {ex.Message}"));
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateReserve([FromBody] ReserveCreateDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(new ApiResponse<string>(false, "Invalid reservation data."));

            try
            {
                var result = await _reservesService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetReservationById), new { id = result.ReserveId },
                    new ApiResponse<ReserveDTO>(true, "Reservation created successfully.", result));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>(false, $"Error creating reservation: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReserveUpdateDTO dto)
        {
            if (id <= 0 || dto == null || id != dto.ReservationId || !ModelState.IsValid)
                return BadRequest(new ApiResponse<string>(false, "Invalid reservation data or ID."));

            try
            {
                var result = await _reservesService.UpdateAsync(id, dto);
                return Ok(new ApiResponse<ReserveDTO>(true, "Reservation updated successfully.", result));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse<string>(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>(false, $"Error updating reservation: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SoftDeleteReservation(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<string>(false, "Invalid reservation ID."));

            try
            {
                var deleted = await _reservesService.SoftDeleteAsync(id);
                if (!deleted)
                    return NotFound(new ApiResponse<string>(false, $"Reservation with ID {id} not found."));

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>(false, $"Error deleting reservation: {ex.Message}"));
            }
        }
    }
}
