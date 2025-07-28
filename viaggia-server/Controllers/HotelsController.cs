using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Hotels;
using viaggia_server.DTOs.Packages;
using viaggia_server.DTOs.Reviews;
using viaggia_server.Models.HotelDates;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Packages;
using viaggia_server.Repositories;
using viaggia_server.Repositories.HotelRepository;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelController : ControllerBase
    {
        private readonly IRepository<Hotel> _hotelRepository;
        private readonly IHotelRepository _hotelRepositorySpecific;

        public HotelController(
            IRepository<Hotel> hotelRepository,
            IHotelRepository hotelRepositorySpecific)
        {
            _hotelRepository = hotelRepository;
            _hotelRepositorySpecific = hotelRepositorySpecific;
        }

        // GET: api/hotel
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllHotels()
        {
            try
            {
                var hotels = await _hotelRepository.GetAllAsync();
                var hotelDTOs = hotels.Select(h => new HotelDTO
                {
                    HotelId = h.HotelId,
                    Name = h.Name,
                    Street = h.Street,
                    Description = h.Description,
                    City = h.City,
                    State = h.State,
                    ZipCode = h.ZipCode,
                    StarRating = h.StarRating,
                    CheckInTime = h.CheckInTime,
                    CheckOutTime = h.CheckOutTime,
                    ContactPhone = h.ContactPhone,
                    ContactEmail = h.ContactEmail,
                    IsActive = h.IsActive,
                    RoomTypes = h.RoomTypes.Select(rt => new HotelRoomTypeDTO
                    {
                        RoomTypeId = rt.RoomTypeId,
                        Name = rt.Name,
                        Price = rt.Price,
                        Capacity = rt.Capacity,
                        BedType = rt.BedType,
                        IsActive = rt.IsActive
                    }).ToList(),
                    HotelDates = h.HotelDates.Select(hd => new HotelDateDTO
                    {
                        HotelDateId = hd.HotelDateId,
                        StartDate = hd.StartDate,
                        EndDate = hd.EndDate,
                        AvailableRooms = hd.AvailableRooms,
                        IsActive = hd.IsActive
                    }).ToList(),
                    Medias = h.Medias.Select(m => new MediaDTO
                    {
                        MediaId = m.MediaId,
                        MediaUrl = m.MediaUrl,
                        MediaType = m.MediaType
                    }).ToList(),
                    Reviews = h.Reviews.Select(r => new ReviewDTO
                    {
                        ReviewId = r.ReviewId,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt
                    }).ToList(),
                    AverageRating = h.Reviews.Any() ? h.Reviews.Average(r => r.Rating) : 0
                }).ToList();

                return Ok(new ApiResponse<List<HotelDTO>>(true, "Hotels retrieved successfully.", hotelDTOs));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<List<HotelDTO>>(false, $"Error retrieving hotels: {ex.Message}"));
            }
        }

        // GET: api/hotel/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHotelById(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<HotelDTO>(false, "Invalid hotel ID."));

            try
            {
                var hotel = await _hotelRepository.GetByIdAsync(id);
                if (hotel == null)
                    return NotFound(new ApiResponse<HotelDTO>(false, $"Hotel with ID {id} not found."));

                hotel.RoomTypes = (await _hotelRepositorySpecific.GetHotelRoomTypesAsync(id)).ToList();
                hotel.HotelDates = (await _hotelRepositorySpecific.GetHotelDatesAsync(id)).ToList();
                hotel.Medias = (await _hotelRepositorySpecific.GetMediasByHotelIdAsync(id)).ToList();
                hotel.Reviews = (await _hotelRepositorySpecific.GetReviewsByHotelIdAsync(id)).ToList();
                hotel.Address = await _hotelRepositorySpecific.GetAddressByHotelIdAsync(id);
                hotel.AverageRating = hotel.Reviews.Any() ? hotel.Reviews.Average(r => r.Rating) : 0;

                var hotelDTO = new HotelDTO
                {
                    HotelId = hotel.HotelId,
                    Name = hotel.Name,
                    Street = hotel.Street,
                    Description = hotel.Description,
                    City = hotel.City,
                    State = hotel.State,
                    ZipCode = hotel.ZipCode,
                    StarRating = hotel.StarRating,
                    CheckInTime = hotel.CheckInTime,
                    CheckOutTime = hotel.CheckOutTime,
                    ContactPhone = hotel.ContactPhone,
                    ContactEmail = hotel.ContactEmail,
                    IsActive = hotel.IsActive,
                    RoomTypes = hotel.RoomTypes.Select(rt => new HotelRoomTypeDTO
                    {
                        RoomTypeId = rt.RoomTypeId,
                        Name = rt.Name,
                        Price = rt.Price,
                        Capacity = rt.Capacity,
                        BedType = rt.BedType,
                        IsActive = rt.IsActive
                    }).ToList(),
                    HotelDates = hotel.HotelDates.Select(hd => new HotelDateDTO
                    {
                        HotelDateId = hd.HotelDateId,
                        StartDate = hd.StartDate,
                        EndDate = hd.EndDate,
                        AvailableRooms = hd.AvailableRooms,
                        IsActive = hd.IsActive
                    }).ToList(),
                    Medias = hotel.Medias.Select(m => new MediaDTO
                    {
                        MediaId = m.MediaId,
                        MediaUrl = m.MediaUrl,
                        MediaType = m.MediaType
                    }).ToList(),
                    Reviews = hotel.Reviews.Select(r => new ReviewDTO
                    {
                        ReviewId = r.ReviewId,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt
                    }).ToList(),
                    AverageRating = hotel.AverageRating
                };

                return Ok(new ApiResponse<HotelDTO>(true, "Hotel retrieved successfully.", hotelDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<HotelDTO>(false, $"Error retrieving hotel: {ex.Message}"));
            }
        }

        // POST: api/hotel
        [HttpPost]
        public async Task<IActionResult> CreateHotel([FromBody] CreateHotelDTO createHotelDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var hotel = new Hotel
            {
                Name = createHotelDto.Name,
                Street = createHotelDto.Street,
                Description = createHotelDto.Description,
                City = createHotelDto.City,
                State = createHotelDto.State,
                ZipCode = createHotelDto.ZipCode,
                StarRating = createHotelDto.StarRating,
                CheckInTime = createHotelDto.CheckInTime,
                CheckOutTime = createHotelDto.CheckOutTime,
                ContactPhone = createHotelDto.ContactPhone,
                ContactEmail = createHotelDto.ContactEmail,
                IsActive = createHotelDto.IsActive
            };

            var createdHotel = await _hotelRepository.AddAsync(hotel);

            // Adicionar HotelDates (se existir)
            if (createHotelDto.HotelDates != null)
            {
                foreach (var hdDto in createHotelDto.HotelDates)
                {
                    var hotelDate = new HotelDate
                    {
                        StartDate = hdDto.StartDate,
                        EndDate = hdDto.EndDate,
                        AvailableRooms = hdDto.AvailableRooms,
                        HotelId = createdHotel.HotelId,
                        IsActive = hdDto.IsActive
                    };
                    await _hotelRepositorySpecific.AddHotelDateAsync(hotelDate);
                }
            }

            // Se houver upload de mídia, trate aqui (não implementado)

            return CreatedAtAction(nameof(GetHotelById), new { id = createdHotel.HotelId }, createdHotel);
        }

        // PUT: api/hotel/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] HotelDTO hotelDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != hotelDto.HotelId) return BadRequest("ID mismatch");

            var existingHotel = await _hotelRepository.GetByIdAsync(id);
            if (existingHotel == null) return NotFound();

            existingHotel.Name = hotelDto.Name;
            existingHotel.Description = hotelDto.Description;
            existingHotel.IsActive = hotelDto.IsActive;

            await _hotelRepository.UpdateAsync(existingHotel);

            // Atualizar RoomTypes e HotelDates também, se desejar

            return NoContent();
        }

        // DELETE: api/hotel/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var deleted = await _hotelRepository.SoftDeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
    