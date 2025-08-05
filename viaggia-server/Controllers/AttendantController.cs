using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Reserve;
using viaggia_server.Repositories.Reserves;

namespace viaggia_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ATTENDANT")]
    public class AttendantController : ControllerBase
    {
        private readonly IReserveRepository _reserveRepository;
        private readonly ILogger<AttendantController> _logger;

        public AttendantController(IReserveRepository reserveRepository, ILogger<AttendantController> logger)
        {
            _reserveRepository = reserveRepository;
            _logger = logger;
        }

        [HttpGet("reservations")]
        [Authorize(Policy = "HotelAccess")]
        public async Task<IActionResult> GetReservations()
        {
            var hotelId = int.Parse(User.FindFirst("HotelId")?.Value ?? "0");
            if (hotelId == 0)
                return BadRequest(new ApiResponse<object>(false, "Usuário não associado a um hotel."));

            var reservations = await _reserveRepository.GetByHotelIdAsync(hotelId);
            var reservationDtos = reservations.Select(r => new ReserveDTO
            {
                ReserveId = r.ReserveId,
                UserId = r.UserId,
                HotelId = r.HotelId, // Nullable int? is safe
                RoomTypeId = r.RoomTypeId, // Nullable int? is safe
                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                NumberOfGuests = r.NumberOfGuests, // Matches Reserve model
                TotalPrice = r.TotalPrice,
                Status = r.Status,
                IsActive = r.IsActive
            }).ToList();

            return Ok(new ApiResponse<List<ReserveDTO>>(true, "Reservas recuperadas com sucesso.", reservationDtos));
        }

        [HttpGet("reservations/{id}")]
        [Authorize(Policy = "HotelAccess")]
        public async Task<IActionResult> GetReservationById(int id)
        {
            var hotelId = int.Parse(User.FindFirst("HotelId")?.Value ?? "0");
            if (hotelId == 0)
                return BadRequest(new ApiResponse<object>(false, "Usuário não associado a um hotel."));

            var reservation = await _reserveRepository.GetByIdAsync(id);
            if (reservation == null || reservation.HotelId != hotelId)
                return NotFound(new ApiResponse<object>(false, "Reserva não encontrada ou não pertence ao hotel."));

            var reservationDto = new ReserveDTO
            {
                ReserveId = reservation.ReserveId,
                UserId = reservation.UserId,
                HotelId = reservation.HotelId,
                RoomTypeId = reservation.RoomTypeId,
                CheckInDate = reservation.CheckInDate,
                CheckOutDate = reservation.CheckOutDate,
                NumberOfGuests= reservation.NumberOfGuests,
                TotalPrice = reservation.TotalPrice,
                Status = reservation.Status,
                IsActive = reservation.IsActive
            };

            return Ok(new ApiResponse<ReserveDTO>(true, "Reserva recuperada com sucesso.", reservationDto));
        }

        [HttpPut("reservations/{id}")]
        [Authorize(Policy = "HotelAccess")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] UpdateReserveDTO dto)
        {
            var hotelId = int.Parse(User.FindFirst("HotelId")?.Value ?? "0");
            if (hotelId == 0)
                return BadRequest(new ApiResponse<object>(false, "Usuário não associado a um hotel."));

            var reservation = await _reserveRepository.GetByIdAsync(id);
            if (reservation == null || reservation.HotelId != hotelId)
                return NotFound(new ApiResponse<object>(false, "Reserva não encontrada ou não pertence ao hotel."));

            reservation.CheckInDate = dto.CheckInDate;
            reservation.CheckOutDate = dto.CheckOutDate;
            reservation.NumberOfGuests = dto.NumberOfPeople; // Matches Reserve model
            reservation.TotalPrice = dto.TotalPrice;
            reservation.Status = dto.Status;

            await _reserveRepository.UpdateAsync(reservation);
            return Ok(new ApiResponse<object>(true, "Reserva atualizada com sucesso."));
        }

        [HttpDelete("reservations/{id}")]
        [Authorize(Policy = "HotelAccess")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var hotelId = int.Parse(User.FindFirst("HotelId")?.Value ?? "0");
            if (hotelId == 0)
                return BadRequest(new ApiResponse<object>(false, "Usuário não associado a um hotel."));

            var reservation = await _reserveRepository.GetByIdAsync(id);
            if (reservation == null || reservation.HotelId != hotelId)
                return NotFound(new ApiResponse<object>(false, "Reserva não encontrada ou não pertence ao hotel."));

            reservation.IsActive = false;
            await _reserveRepository.UpdateAsync(reservation);
            return Ok(new ApiResponse<object>(true, "Reserva cancelada com sucesso."));
        }
    }
}