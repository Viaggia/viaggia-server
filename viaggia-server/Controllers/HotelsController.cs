using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Hotel;
using viaggia_server.DTOs.HotelFilterDTO;
using viaggia_server.DTOs.Hotels;
using viaggia_server.Services.HotelServices;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelController : ControllerBase
    {
        private readonly IHotelServices _hotelServices;
        private readonly ILogger<HotelController> _logger;

        public HotelController(IHotelServices hotelServices, ILogger<HotelController> logger)
        {
            _hotelServices = hotelServices;
            _logger = logger;
        }

        [HttpPost("create")]
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
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
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