using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Reservation;
using viaggia_server.Models.Reservations;
using viaggia_server.Repositories;
using viaggia_server.Repositories.Reservations;

namespace viaggia_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly IRepository<Reservation> _genericRepository;
        private readonly IReservationRepository _reservationRepository;

        public ReservationsController(IRepository<Reservation> genericRepo, IReservationRepository reservationRepo)
        {
            _genericRepository = genericRepo ?? throw new ArgumentNullException(nameof(genericRepo));
            _reservationRepository = reservationRepo ?? throw new ArgumentNullException(nameof(reservationRepo));
        }

        /// <summary>
        /// Retrieves all active reservations.
        /// </summary>
        /// <returns>A list of active reservations.</returns>
        /// <response code="200">Returns the list of reservations.</response>
        /// <response code="500">If an error occurs while retrieving reservations.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReservations()
        {
            try
            {
                var reservations = await _genericRepository.GetAllAsync();
                var reservationDTOs = reservations.Select(r => new ReservationDTO
                {
                    ReservationId = r.ReservationId,
                    UserId = r.UserId,
                    PackageId = r.PackageId,
                    RoomTypeId = r.RoomTypeId,
                    HotelId = r.HotelId,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    TotalPrice = r.TotalPrice,
                    NumberOfGuests = r.NumberOfGuests,
                    Status = r.Status,
                    IsActive = r.IsActive
                }).ToList();

                return Ok(new ApiResponse(true, "Reservations retrieved successfully.", reservationDTOs));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(false, $"Error retrieving reservations: {ex.Message}"));
            }
        }

        /// <summary>
        /// Retrieves an active reservation by its ID.
        /// </summary>
        /// <param name="id">The ID of the reservation.</param>
        /// <returns>The reservation with the specified ID.</returns>
        /// <response code="200">Returns the reservation.</response>
        /// <response code="400">If the ID is invalid.</response>
        /// <response code="404">If the reservation is not found.</response>
        /// <response code="500">If an error occurs while retrieving the reservation.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReservationById(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse(false, "Invalid reservation ID."));

            try
            {
                var reservation = await _reservationRepository.GetReservationByIdAsync(id);
                if (reservation == null)
                    return NotFound(new ApiResponse(false, $"Reservation with ID {id} not found."));

                var dto = new ReservationDTO
                {
                    ReservationId = reservation.ReservationId,
                    UserId = reservation.UserId,
                    PackageId = reservation.PackageId,
                    RoomTypeId = reservation.RoomTypeId,
                    HotelId = reservation.HotelId,
                    StartDate = reservation.StartDate,
                    EndDate = reservation.EndDate,
                    TotalPrice = reservation.TotalPrice,
                    NumberOfGuests = reservation.NumberOfGuests,
                    Status = reservation.Status,
                    IsActive = reservation.IsActive
                };

                return Ok(new ApiResponse(true, "Reservation retrieved successfully.", dto));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(false, $"Error retrieving reservation: {ex.Message}"));
            }
        }


        /// <summary>
        /// Creates a new reservation.
        /// </summary>
        /// <param name="reservationDTO">The reservation data.</param>
        /// <returns>The created reservation.</returns>
        /// <response code="201">Returns the newly created reservation.</response>
        /// <response code="400">If the reservation data is invalid.</response>
        /// <response code="500">If an error occurs while creating the reservation.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateReservation([FromBody] ReservationCreateDTO reservationDTO)
        {
            if (reservationDTO == null || !ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid reservation data.", ModelState));

            try
            {
                var reservation = new Reservation
                {
                    UserId = reservationDTO.UserId,
                    PackageId = reservationDTO.PackageId,
                    RoomTypeId = reservationDTO.RoomTypeId,
                    HotelId = reservationDTO.HotelId,
                    StartDate = reservationDTO.StartDate,
                    EndDate = reservationDTO.EndDate,
                    TotalPrice = reservationDTO.TotalPrice,
                    NumberOfGuests = reservationDTO.NumberOfGuests,
                    Status = reservationDTO.Status,
                    IsActive = reservationDTO.IsActive
                };

                await _genericRepository.AddAsync(reservation);
                await _genericRepository.SaveChangesAsync();

                var resultDTO = new ReservationDTO
                {
                    ReservationId = reservation.ReservationId,
                    UserId = reservation.UserId,
                    PackageId = reservation.PackageId,
                    RoomTypeId = reservation.RoomTypeId,
                    HotelId = reservation.HotelId,
                    StartDate = reservation.StartDate,
                    EndDate = reservation.EndDate,
                    TotalPrice = reservation.TotalPrice,
                    NumberOfGuests = reservation.NumberOfGuests,
                    Status = reservation.Status,
                    IsActive = reservation.IsActive
                };

                return CreatedAtAction(nameof(GetReservationById), new { id = reservation.ReservationId },
                    new ApiResponse(true, "Reservation created successfully.", resultDTO));
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(false, $"Error creating reservation: {ex.Message} | Inner: {inner}"));
            

            //return StatusCode(StatusCodes.Status500InternalServerError,
              //      new ApiResponse(false, $"Error creating reservation: {ex.Message}"));
            }
        }

        /// <summary>
        /// Updates an existing reservation.
        /// </summary>
        /// <param name="id">The ID of the reservation.</param>
        /// <param name="reservationDTO">The updated reservation data.</param>
        /// <returns>The updated reservation.</returns>
        /// <response code="200">Returns the updated reservation.</response>
        /// <response code="400">If the reservation data or ID is invalid.</response>
        /// <response code="404">If the reservation is not found.</response>
        /// <response code="500">If an error occurs while updating the reservation.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationUpdateDTO reservationDTO)
        {
            if (id <= 0 || reservationDTO == null || id != reservationDTO.ReservationId || !ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid reservation data or ID."));

            try
            {
                var reservation = await _reservationRepository.GetReservationByIdAsync(id);
                if (reservation == null)
                    return NotFound(new ApiResponse(false, $"Reservation with ID {id} not found."));

                reservation.UserId = reservationDTO.UserId;
                reservation.PackageId = reservationDTO.PackageId;
                reservation.RoomTypeId = reservationDTO.RoomTypeId;
                reservation.HotelId = reservationDTO.HotelId;
                reservation.StartDate = reservationDTO.StartDate;
                reservation.EndDate = reservationDTO.EndDate;
                reservation.TotalPrice = reservationDTO.TotalPrice;
                reservation.NumberOfGuests = reservationDTO.NumberOfGuests;
                reservation.Status = reservationDTO.Status;
                reservation.IsActive = reservationDTO.IsActive;

                await _genericRepository.UpdateAsync(reservation);
                await _genericRepository.SaveChangesAsync();

                var resultDTO = new ReservationDTO
                {
                    ReservationId = reservation.ReservationId,
                    UserId = reservation.UserId,
                    PackageId = reservation.PackageId,
                    RoomTypeId = reservation.RoomTypeId,
                    HotelId = reservation.HotelId,
                    StartDate = reservation.StartDate,
                    EndDate = reservation.EndDate,
                    TotalPrice = reservation.TotalPrice,
                    NumberOfGuests = reservation.NumberOfGuests,
                    Status = reservation.Status,
                    IsActive = reservation.IsActive
                };

                return Ok(new ApiResponse(true, "Reservation updated successfully.", resultDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(false, $"Error updating reservation: {ex.Message}"));
            }
        }

        /// <summary>
        /// Soft deletes a reservation by its ID.
        /// </summary>
        /// <param name="id">The ID of the reservation.</param>
        /// <returns>Confirmation of deletion.</returns>
        /// <response code="204">Reservation soft deleted successfully.</response>
        /// <response code="400">If the ID is invalid.</response>
        /// <response code="404">If the reservation is not found.</response>
        /// <response code="500">If an error occurs while deleting the reservation.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SoftDeleteReservation(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse(false, "Invalid reservation ID."));

            try
            {
                var deleted = await _genericRepository.SoftDeleteAsync(id);
                if (!deleted)
                    return NotFound(new ApiResponse(false, $"Reservation with ID {id} not found."));

                await _genericRepository.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(false, $"Error deleting reservation: {ex.Message}"));
            }
        }

    }
}