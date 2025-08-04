using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Hotel;
using viaggia_server.DTOs.Reviews;
using viaggia_server.Services.HotelServices;

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

        [HttpPost("{hotelId}/reviews")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateReview(int hotelId, [FromBody] CreateReviewDTO reviewDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("ModelState is invalid: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                // Ensure hotelId from URL matches reviewDto.HotelId
                if (reviewDto.HotelId != hotelId)
                {
                    _logger.LogWarning("HotelId mismatch: URL HotelId {UrlHotelId}, DTO HotelId {DtoHotelId}", hotelId, reviewDto.HotelId);
                    return BadRequest(new ApiResponse<ReviewDTO>(false, "HotelId in URL must match HotelId in request body."));
                }

                var response = await _hotelServices.AddHotelReviewAsync(reviewDto);
                if (!response.Success)
                {
                    _logger.LogError("Failed to create review: {Message}", response.Message);
                    return BadRequest(response.Message);
                }

                return CreatedAtAction(nameof(GetReviewsByHotelId), new { hotelId = response.Data.HotelId }, response.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for HotelId: {HotelId}", hotelId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<ReviewDTO>(false, $"Error creating review: {ex.Message}"));
            }
        }

        [HttpGet("{hotelId}/reviews")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReviewsByHotelId(int hotelId)
        {
            try
            {
                var response = await _hotelServices.GetHotelReviewsAsync(hotelId);
                if (!response.Success)
                {
                    _logger.LogWarning("Failed to retrieve reviews: {Message}", response.Message);
                    return BadRequest(new ApiResponse<List<ReviewDTO>>(false, response.Message));
                }

                if (!response.Data.Any())
                {
                    _logger.LogInformation("No reviews found for HotelId: {HotelId}", hotelId);
                    return NotFound(new ApiResponse<List<ReviewDTO>>(false, "No reviews found."));
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for HotelId: {HotelId}", hotelId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<List<ReviewDTO>>(false, $"Error retrieving reviews: {ex.Message}"));
            }
        }

        [HttpPut("reviews/{reviewId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] CreateReviewDTO reviewDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("ModelState is invalid: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var response = await _hotelServices.UpdateHotelReviewAsync(reviewId, reviewDto);
                if (!response.Success)
                {
                    _logger.LogError("Failed to update review: {Message}", response.Message);
                    return BadRequest(response.Message);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review with ReviewId: {ReviewId}", reviewId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<ReviewDTO>(false, $"Error updating review: {ex.Message}"));
            }
        }

        [HttpDelete("reviews/{reviewId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            try
            {
                var response = await _hotelServices.RemoveHotelReviewAsync(reviewId);
                if (!response.Success)
                {
                    _logger.LogError("Failed to delete review: {Message}", response.Message);
                    return BadRequest(response.Message);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review with ReviewId: {ReviewId}", reviewId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<bool>(false, $"Error deleting review: {ex.Message}"));
            }
        }

        // Existing endpoints (unchanged)
        [HttpGet("filter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FilterHotels(
            [FromQuery] List<string> commodities,
            [FromQuery] List<string> CustomCommodities,
            [FromQuery] List<string> roomTypes,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int? minCapacity)
        {
            try
            {
                if ((commodities == null || !commodities.Any()) &&
                    (CustomCommodities == null || !CustomCommodities.Any()) &&
                    (roomTypes == null || !roomTypes.Any()) &&
                    !minPrice.HasValue && !maxPrice.HasValue && !minCapacity.HasValue)
                {
                    _logger.LogWarning("At least one filter parameter must be provided.");
                    return BadRequest(new ApiResponse<List<HotelDTO>>(false, "At least one filter parameter must be provided."));
                }

                var filter = new HotelFilterDTO
                {
                    Commodity = commodities ?? new List<string>(),
                    CustomCommodity = CustomCommodities ?? new List<string>(),
                    RoomTypes = roomTypes ?? new List<string>(),
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    MinCapacity = minCapacity
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

        [HttpGet("{hotelId}/available-rooms")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAvailableRooms(
            int hotelId,
            [FromQuery] int numberOfPeople,
            [FromQuery] DateTime checkInDate,
            [FromQuery] DateTime checkOutDate)
        {
            try
            {
                var response = await _hotelServices.GetAvailableRoomsAsync(hotelId, numberOfPeople, checkInDate, checkOutDate);
                if (!response.Success)
                {
                    _logger.LogWarning("Failed to retrieve available rooms: {Message}", response.Message);
                    return BadRequest(new ApiResponse<List<HotelRoomTypeDTO>>(false, response.Message));
                }

                if (!response.Data.Any())
                {
                    _logger.LogInformation("No available rooms found for HotelId: {HotelId}, People: {NumberOfPeople}, CheckIn: {CheckInDate}, CheckOut: {CheckOutDate}",
                        hotelId, numberOfPeople, checkInDate, checkOutDate);
                    return NotFound(new ApiResponse<List<HotelRoomTypeDTO>>(false, "No available rooms found."));
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available rooms for HotelId: {HotelId}", hotelId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<List<HotelRoomTypeDTO>>(false, $"Error retrieving available rooms: {ex.Message}"));
            }
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
            var response = await _hotelServices.GetHotelByIdAsync(id);
            if (!response.Success)
                return NotFound(response.Message);
            return Ok(response.Data);
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

       
    }
}