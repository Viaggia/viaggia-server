using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
<<<<<<< HEAD
using viaggia_server.DTOs.Commoditie;
using viaggia_server.DTOs.Hotels;
using viaggia_server.DTOs.Packages;
using viaggia_server.DTOs.Reviews;
using viaggia_server.Models.HotelDates;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Repositories;
using viaggia_server.Repositories.HotelRepository;
=======
using viaggia_server.DTOs.Hotel;
using viaggia_server.DTOs.HotelFilterDTO;
using viaggia_server.DTOs.Hotels;
using viaggia_server.Services.HotelServices;
>>>>>>> 4ab8ac3dc4732ca91d9c662fc8b90e047b46890d

namespace viaggia_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        private readonly IHotelServices _hotelServices;
        private readonly ILogger<HotelController> _logger;

        public HotelController(IHotelServices hotelServices, ILogger<HotelController> logger)
        {
            _hotelServices = hotelServices;
            _logger = logger;
        }

<<<<<<< HEAD
        // GET: api/hotel
        [HttpGet]
        public async Task<IActionResult> GetAllHotels()
        {
            try
            {
                var hotels = await _hotelRepository.GetAllHotelsWithDetailsAsync();
                var hotelDTOs = new List<HotelDTO>();
                foreach (var hotel in hotels)
                {
                    var roomTypes = (await _hotelRepository.GetHotelRoomTypesAsync(hotel.HotelId)).ToList();
                    var hotelDates = (await _hotelRepository.GetHotelDatesAsync(hotel.HotelId)).ToList();
                    var medias = (await _hotelRepository.GetMediasByHotelIdAsync(hotel.HotelId)).ToList();
                    var reviews = (await _hotelRepository.GetReviewsByHotelIdAsync(hotel.HotelId)).ToList();
                    var packages = (await _hotelRepository.GetPackagesByHotelIdAsync(hotel.HotelId)).ToList();

                    var dto = new HotelDTO
                    {
                        HotelId = hotel.HotelId,
                        Name = hotel.Name,
                        Description = hotel.Description,
                        StarRating = hotel.StarRating,
                        CheckInTime = hotel.CheckInTime,
                        CheckOutTime = hotel.CheckOutTime,
                        ContactPhone = hotel.ContactPhone,
                        ContactEmail = hotel.ContactEmail,
                        IsActive = hotel.IsActive,

                        RoomTypes = roomTypes.Select(rt => new HotelRoomTypeDTO
                        {
                            RoomTypeId = rt.RoomTypeId,
                            Name = rt.Name,
                            Price = rt.Price,
                            Capacity = rt.Capacity,
                            BedType = rt.BedType,
                            IsActive = rt.IsActive
                        }).ToList(),

                        HotelDates = hotelDates.Select(hd => new HotelDateDTO
                        {
                            HotelDateId = hd.HotelDateId,
                            StartDate = hd.StartDate,
                            EndDate = hd.EndDate,
                            AvailableRooms = hd.AvailableRooms,
                            IsActive = hd.IsActive
                        }).ToList(),

                        Medias = medias.Select(m => new MediaDTO
                        {
                            MediaId = m.MediaId,
                            MediaUrl = m.MediaUrl,
                            MediaType = m.MediaType
                        }).ToList(),

                        Reviews = reviews.Select(r => new ReviewDTO
                        {
                            ReviewId = r.ReviewId,
                            Rating = r.Rating,
                            Comment = r.Comment,
                            CreatedAt = r.CreatedAt
                        }).ToList(),
                        
                        Packages = packages.Select(p => new PackageDTO
                        {
                            PackageId = p.PackageId,
                            Name = p.Name,
                            Description = p.Description,
                            BasePrice = p.BasePrice,
                            IsActive = p.IsActive
                        }).ToList(),

                        AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,

                    };

                    hotelDTOs.Add(dto);
                }

                return Ok(new ApiResponse<List<HotelDTO>>(true, "Hotels retrieved successfully.", hotelDTOs));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<Hotel>(false, $"Erro ao criar hotel: {innerMessage}"));
            }
        }

        // GET: api/hotel/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<HotelDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<HotelDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHotelById(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<HotelDTO>(false, "Invalid hotel ID."));

            try
            {
                var hotel = await _hotelRepository.GetHotelByIdWithDetailsAsync(id);
                if (hotel == null)
                    return NotFound(new ApiResponse<HotelDTO>(false, $"Hotel with ID {id} not found."));

                hotel.RoomTypes = (await _hotelRepository.GetHotelRoomTypesAsync(id)).ToList();
                hotel.HotelDates = (await _hotelRepository.GetHotelDatesAsync(id)).ToList();
                hotel.Medias = (await _hotelRepository.GetMediasByHotelIdAsync(id)).ToList();
                hotel.Reviews = (await _hotelRepository.GetReviewsByHotelIdAsync(id)).ToList();

                hotel.AverageRating = hotel.Reviews.Any() ? hotel.Reviews.Average(r => r.Rating) : 0;

                var hotelDTO = new HotelDTO
                {
                    HotelId = hotel.HotelId,
                    Name = hotel.Name,
                  
                    Description = hotel.Description,
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
                    Packages = hotel.Packages.Select(p => new PackageDTO
                    {
                        PackageId = p.PackageId,
                        Name = p.Name,
                        Description = p.Description,
                        BasePrice = p.BasePrice,
                        IsActive = p.IsActive
                    }).ToList(),
                    AverageRating = hotel.AverageRating

                };

                return Ok(new ApiResponse<HotelDTO>(true, "Hotel retrieved successfully.", hotelDTO));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<Hotel>(false, $"Erro ao criar hotel: {innerMessage}"));
            }
        }

        [HttpPost]
=======
        [HttpPost("create")]
>>>>>>> 4ab8ac3dc4732ca91d9c662fc8b90e047b46890d
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateHotel([FromForm] CreateHotelDTO createHotelDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("ModelState is invalid: {Errors}", ModelState);
                return BadRequest(ModelState);
            }

            List<CreateHotelRoomTypeDTO> roomTypes;

            try
            {
                var options = new JsonSerializerOptions
                {
<<<<<<< HEAD
                    foreach (var file in createHotelDto.MediaFiles)
                    {
                        var ext = Path.GetExtension(file.FileName).ToLower();
                        if (!allowedExtensions.Contains(ext) || file.Length > 5 * 1024 * 1024)
                            return BadRequest(new ApiResponse<CreateHotelDTO>(false, "Invalid media file."));
                    }
                }

                // Criar hotel base
                var hotel = new Hotel
                {
                    Name = createHotelDto.Name,
                    Description = createHotelDto.Description,
                    StarRating = createHotelDto.StarRating,
                    CheckInTime = createHotelDto.CheckInTime,
                    CheckOutTime = createHotelDto.CheckOutTime,
                    ContactPhone = createHotelDto.ContactPhone,
                    ContactEmail = createHotelDto.ContactEmail,
                    IsActive = createHotelDto.IsActive,
                    Cnpj = createHotelDto.Cnpj
=======
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
>>>>>>> 4ab8ac3dc4732ca91d9c662fc8b90e047b46890d
                };

                roomTypes = JsonSerializer.Deserialize<List<CreateHotelRoomTypeDTO>>(createHotelDto.RoomTypesJson, options)!;

                if (roomTypes == null || !roomTypes.Any())
                {
                    _logger.LogWarning("No room types provided in JSON.");
                    return BadRequest("At least one room type must be provided.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid RoomTypesJson");
                return BadRequest("RoomTypesJson is invalid or malformed.");
            }

            var response = await _hotelServices.CreateHotelAsync(createHotelDto, roomTypes);

            if (!response.Success)
            {
                _logger.LogError("Failed to create hotel: {Message}", response.Message);
                return BadRequest(response.Message);
            }

            return CreatedAtAction(nameof(GetHotelById), new { id = response.Data.HotelId }, response.Data);
        }


        [HttpGet("getAll")]
        public async Task<IActionResult> GetAllHotels()
        {
            var response = await _hotelServices.GetAllHotelAsync();
            if (!response.Success)
                return BadRequest(response.Message);
            return Ok(response.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHotelById(int id)
        {
            var hotel = await _hotelServices.GetHotelByIdAsync(id);
            if (hotel == null)
                return NotFound("Hotel not found.");
            return Ok(hotel);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateHotel([FromBody] UpdateHotelDto updateHotelDto)
        {
            try
            {
                var hotel = await _hotelServices.UpdateHotelAsync(updateHotelDto);
                return Ok(hotel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var result = await _hotelServices.SoftDeleteHotelAsync(id);
            if (!result)
                return NotFound("Hotel not found.");
            return NoContent();
        }

        [HttpGet("{hotelId}/averageRating")]
        public async Task<IActionResult> GetHotelAverageRating(int hotelId)
        {
            var response = await _hotelServices.GetHotelAverageRatingAsync(hotelId);
            if (!response.Success)
                return BadRequest(response.Message);
            return Ok(response.Data);
        }

        [HttpGet("{hotelId}/packages")]
        public async Task<IActionResult> GetPackagesByHotelId(int hotelId)
        {
            var response = await _hotelServices.GetPackagesByHotelIdAsync(hotelId);
            if (!response.Success)
                return BadRequest(response.Message);
            return Ok(response.Data);
        }

        [HttpGet("filter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FilterHotels([FromQuery] List<string> commodities, [FromQuery] List<string> commoditieServices, [FromQuery] List<string> roomTypes)
        {
            try
            {
                if ((commodities == null || !commodities.Any()) &&
                    (commoditieServices == null || !commoditieServices.Any()) &&
                    (roomTypes == null || !roomTypes.Any()))
                {
                    _logger.LogWarning("At least one filter parameter (commodities, commoditieServices, or roomTypes) must be provided.");
                    return BadRequest(new ApiResponse<List<HotelDTO>>(false, "At least one filter parameter must be provided."));
                }

                var filter = new HotelFilterDTO
                {
                    Commodities = commodities ?? new List<string>(),
                    CommoditieServices = commoditieServices ?? new List<string>(),
                    RoomTypes = roomTypes ?? new List<string>()
                };

                var response = await _hotelServices.FilterHotelsAsync(filter);
                if (!response.Success)
                {
                    _logger.LogWarning("Failed to filter hotels: {Message}", response.Message);
                    return BadRequest(new ApiResponse<List<HotelDTO>>(false, response.Message));
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering hotels");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<List<HotelDTO>>(false, $"Error filtering hotels: {ex.Message}"));
            }
        }
    }
}