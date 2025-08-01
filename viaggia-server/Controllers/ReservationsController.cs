using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Reservation;
using viaggia_server.Services.Reservations;

namespace viaggia_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReservations()
        {
            try
            {
                var result = await _reservationService.GetAllAsync();
                return Ok(new ApiResponse<List<ReservationDTO>>(true, "Reservations retrieved successfully.", result));
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
                var result = await _reservationService.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new ApiResponse<string>(false, $"Reservation with ID {id} not found."));

                return Ok(new ApiResponse<ReservationDTO>(true, "Reservation retrieved successfully.", result));
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
        public async Task<IActionResult> CreateReservation([FromBody] ReservationCreateDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(new ApiResponse<string>(false, "Invalid reservation data."));

            try
            {
                var result = await _reservationService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetReservationById), new { id = result.ReservationId },
                    new ApiResponse<ReservationDTO>(true, "Reservation created successfully.", result));
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
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationUpdateDTO dto)
        {
            if (id <= 0 || dto == null || id != dto.ReservationId || !ModelState.IsValid)
                return BadRequest(new ApiResponse<string>(false, "Invalid reservation data or ID."));

            try
            {
                var result = await _reservationService.UpdateAsync(id, dto);
                return Ok(new ApiResponse<ReservationDTO>(true, "Reservation updated successfully.", result));
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
                var deleted = await _reservationService.SoftDeleteAsync(id);
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
