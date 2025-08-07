using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Hotel;
using viaggia_server.DTOs.Packages;
using viaggia_server.DTOs.Reserves;
using viaggia_server.DTOs.Reviews;
using viaggia_server.Models.Hotels;
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

        // GET: api/Hotel
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Changed from BadRequest to NotFound for consistency
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllHotels()
        {
            try
            {
                var response = await _hotelServices.GetAllHotelAsync();
                if (!response.Success)
                {
                    _logger.LogWarning("Failed to retrieve hotels: {Message}", response.Message);
                    return NotFound(new ApiResponse<List<HotelDTO>>(false, response.Message));
                }

                if (!response.Data.Any())
                {
                    _logger.LogInformation("No hotels found.");
                    return NotFound(new ApiResponse<List<HotelDTO>>(false, "No hotels found."));
                }

                _logger.LogInformation("Retrieved {Count} hotels by Admin", response.Data.Count);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotels for Admin");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<List<HotelDTO>>(false, $"Error retrieving hotels: {ex.Message}"));
            }
        }

        // GET: api/Hotel/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHotelById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid hotel ID: {HotelId}", id);
                    return BadRequest(new ApiResponse<HotelDTO>(false, "Invalid hotel ID."));
                }

                var response = await _hotelServices.GetHotelByIdAsync(id);
                if (!response.Success)
                {
                    _logger.LogWarning("Failed to retrieve hotel with ID {HotelId}: {Message}", id, response.Message);
                    return NotFound(new ApiResponse<HotelDTO>(false, response.Message));
                }

                _logger.LogInformation("Hotel ID {HotelId} retrieved by {Role}", id, User.IsInRole("ADMIN") ? "Admin" : "Service Provider");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotel with ID: {HotelId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<HotelDTO>(false, $"Error retrieving hotel: {ex.Message}"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "SERVICE_PROVIDER, ADMIN")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateHotel([FromForm] CreateHotelDTO createHotelDto)
        {
            _logger.LogInformation("CreateHotelDTO recebido: {DTO}", JsonSerializer.Serialize(createHotelDto));
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("ModelState is invalid: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Invalid or missing UserId in claims");
                    return Unauthorized(new ApiResponse<HotelDTO>(false, "User not authenticated."));
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
                        return BadRequest(new ApiResponse<HotelDTO>(false, "At least one room type must be provided."));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Invalid RoomTypesJson");
                    return BadRequest(new ApiResponse<HotelDTO>(false, "RoomTypesJson is invalid or malformed."));
                }

                var response = await _hotelServices.CreateHotelAsync(createHotelDto, roomTypes, userId);
                if (!response.Success)
                {
                    _logger.LogError("Failed to create hotel: {Message}", response.Message);
                    return BadRequest(new ApiResponse<HotelDTO>(false, response.Message));
                }

                return CreatedAtAction(nameof(GetHotelById), new { id = response.Data.HotelId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating hotel");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<HotelDTO>(false, $"Error creating hotel: {ex.Message}"));
            }
        }

        // PUT: api/Hotel/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,SERVICE_PROVIDER,ATTENDANT")] // Allow ADMIN, SERVICE_PROVIDER, ATTENDANT
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateHotel(int id, [FromForm] UpdateHotelDto updateHotelDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("ModelState is invalid: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                if (id != updateHotelDto.HotelId)
                {
                    _logger.LogWarning("HotelId mismatch: URL HotelId {UrlHotelId}, DTO HotelId {DtoHotelId}", id, updateHotelDto.HotelId);
                    return BadRequest(new ApiResponse<HotelDTO>(false, "HotelId in URL must match HotelId in request body."));
                }

                // Authorization check for SERVICE_PROVIDER and ATTENDANT
                if (!User.IsInRole("ADMIN"))
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    {
                        _logger.LogWarning("Invalid or missing UserId in claims for updating HotelId {HotelId}", id);
                        return Unauthorized(new ApiResponse<HotelDTO>(false, "User not authenticated."));
                    }

                    var hotel = await _hotelServices.GetHotelByIdAsync(id);
                    if (!hotel.Success || hotel.Data == null)
                    {
                        _logger.LogWarning("Hotel not found for ID {HotelId}", id);
                        return NotFound(new ApiResponse<HotelDTO>(false, "Hotel not found."));
                    }

                    if (hotel.Data.UserId != userId)
                    {
                        _logger.LogWarning("User {UserId} attempted to update hotel {HotelId} they do not own", userId, id);
                        return StatusCode(StatusCodes.Status403Forbidden,
                            new ApiResponse<HotelDTO>(false, "You can only update hotels you own."));
                    }
                }

                List<CreateHotelRoomTypeDTO>? roomTypes = null;
                if (!string.IsNullOrEmpty(updateHotelDto.RoomTypesJson))
                {
                    try
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            Converters = { new JsonStringEnumConverter() }
                        };
                        roomTypes = JsonSerializer.Deserialize<List<CreateHotelRoomTypeDTO>>(updateHotelDto.RoomTypesJson, options);
                        if (roomTypes == null || !roomTypes.Any())
                        {
                            _logger.LogWarning("RoomTypesJson provided but empty or invalid for HotelId {HotelId}", id);
                            return BadRequest(new ApiResponse<HotelDTO>(false, "RoomTypesJson is empty or invalid."));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Invalid RoomTypesJson for HotelId: {HotelId}", id);
                        return BadRequest(new ApiResponse<HotelDTO>(false, "RoomTypesJson is invalid or malformed."));
                    }
                }

                var response = await _hotelServices.UpdateHotelAsync(updateHotelDto, roomTypes);
                if (!response.Success)
                {
                    _logger.LogWarning("Failed to update hotel with ID {HotelId}: {Message}", id, response.Message);
                    return BadRequest(new ApiResponse<HotelDTO>(false, response.Message));
                }

                _logger.LogInformation("Hotel ID {HotelId} updated by {Role}", id, User.IsInRole("ADMIN") ? "Admin" : User.IsInRole("SERVICE_PROVIDER") ? "Service Provider" : "Attendant");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hotel with ID: {HotelId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<HotelDTO>(false, $"Error updating hotel: {ex.Message}"));
            }
        }

        // DELETE: api/Hotel/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN,SERVICE_PROVIDER,ATTENDANT")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid hotel ID: {HotelId}", id);
                    return BadRequest(new ApiResponse<bool>(false, "Invalid hotel ID."));
                }

                // Authorization check for SERVICE_PROVIDER and ATTENDANT
                if (!User.IsInRole("ADMIN"))
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    {
                        _logger.LogWarning("Invalid or missing UserId in claims for deleting HotelId {HotelId}", id);
                        return Unauthorized(new ApiResponse<bool>(false, "User not authenticated."));
                    }

                    var hotel = await _hotelServices.GetHotelByIdAsync(id);
                    if (!hotel.Success || hotel.Data == null)
                    {
                        _logger.LogWarning("Hotel not found for ID {HotelId}", id);
                        return NotFound(new ApiResponse<bool>(false, "Hotel not found."));
                    }

                    if (hotel.Data.UserId != userId)
                    {
                        _logger.LogWarning("User {UserId} attempted to delete hotel {HotelId} they do not own", userId, id);
                        return StatusCode(StatusCodes.Status403Forbidden,
                            new ApiResponse<bool>(false, "You can only delete hotels you own."));
                    }
                }

                var result = await _hotelServices.SoftDeleteHotelAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Hotel not found with ID: {HotelId}", id);
                    return NotFound(new ApiResponse<bool>(false, "Hotel not found."));
                }

                _logger.LogInformation("Hotel ID {HotelId} deleted by {Role}", id, User.IsInRole("ADMIN") ? "Admin" : User.IsInRole("SERVICE_PROVIDER") ? "Service Provider" : "Attendant");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting hotel with ID: {HotelId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<bool>(false, $"Error deleting hotel: {ex.Message}"));
            }
        }

        // GET: api/Hotel/{hotelId}/reviews
        [HttpGet("{hotelId}/reviews")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        // POST: api/Hotel/{hotelId}/reviews
        [HttpPost("{hotelId}/reviews")]
        [Authorize(Roles = "CLIENT")] // Restrict to CLIENT
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

                if (reviewDto.HotelId != hotelId)
                {
                    _logger.LogWarning("HotelId mismatch: URL HotelId {UrlHotelId}, DTO HotelId {DtoHotelId}", hotelId, reviewDto.HotelId);
                    return BadRequest(new ApiResponse<ReviewDTO>(false, "HotelId in URL must match HotelId in request body."));
                }

                // Set UserId from authenticated client
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Invalid or missing UserId in claims for creating review for HotelId {HotelId}", hotelId);
                    return Unauthorized(new ApiResponse<ReviewDTO>(false, "User not authenticated."));
                }
                reviewDto.UserId = userId;

                var response = await _hotelServices.AddHotelReviewAsync(reviewDto);
                if (!response.Success)
                {
                    _logger.LogError("Failed to create review for HotelId {HotelId}: {Message}", hotelId, response.Message);
                    return BadRequest(new ApiResponse<ReviewDTO>(false, response.Message));
                }

                _logger.LogInformation("Review created for HotelId {HotelId} by Client UserId {UserId}", hotelId, userId);
                return CreatedAtAction(nameof(GetReviewsByHotelId), new { hotelId = response.Data.HotelId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for HotelId: {HotelId}", hotelId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<ReviewDTO>(false, $"Error creating review: {ex.Message}"));
            }
        }

        // PUT: api/Hotel/reviews/{reviewId}
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
                    return BadRequest(new ApiResponse<ReviewDTO>(false, response.Message));
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

        // DELETE: api/Hotel/reviews/{reviewId}
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
                    return BadRequest(new ApiResponse<bool>(false, response.Message));
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

        // GET: api/Hotel/{hotelId}/packages
        [HttpGet("{hotelId}/packages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPackagesByHotelId(int hotelId)
        {
            try
            {
                var response = await _hotelServices.GetPackagesByHotelIdAsync(hotelId);
                if (!response.Success)
                {
                    _logger.LogWarning("Failed to retrieve packages: {Message}", response.Message);
                    return BadRequest(new ApiResponse<IEnumerable<PackageDTO>>(false, response.Message));
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving packages for HotelId: {HotelId}", hotelId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<IEnumerable<PackageDTO>>(false, $"Error retrieving packages: {ex.Message}"));
            }
        }

        // GET: api/Hotel/{hotelId}/averageRating
        [HttpGet("{hotelId}/averageRating")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHotelAverageRating(int hotelId)
        {
            try
            {
                var response = await _hotelServices.GetHotelAverageRatingAsync(hotelId);
                if (!response.Success)
                {
                    _logger.LogWarning("Failed to retrieve average rating: {Message}", response.Message);
                    return BadRequest(new ApiResponse<double>(false, response.Message));
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving average rating for HotelId: {HotelId}", hotelId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<double>(false, $"Error retrieving average rating: {ex.Message}"));
            }
        }

        // GET: api/Hotel/{hotelId}/available-rooms
        [HttpGet("{hotelId}/available-rooms")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        // GET: api/Hotel/filter
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

        // GET: api/Hotel/search
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchHotels([FromQuery] HotelSearchDTO searchDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("ModelState is invalid: {Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                var response = await _hotelServices.SearchHotelsByDestinationAsync(searchDto);
                if (response.Data == null || !response.Data.Any())
                {
                    _logger.LogInformation("No available hotels found for City: {City}, People: {NumberOfPeople}, Rooms: {NumberOfRooms}",
                        searchDto.City, searchDto.NumberOfPeople, searchDto.NumberOfRooms);
                    return Ok(new ApiResponse<List<HotelDTO>>(true, "No available hotels found.", new List<HotelDTO>()));
                }

                if (!response.Success)
                {
                    _logger.LogWarning("Failed to retrieve hotels: {Message}", response.Message);
                    return BadRequest(new ApiResponse<List<HotelDTO>>(false, response.Message));
                }

                

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching hotels for City: {City}", searchDto.City);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<List<HotelDTO>>(false, $"Error searching hotels: {ex.Message}"));
            }
        }

        // GET: api/Hotel/user/{userId}
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "ADMIN,SERVICE_PROVIDER")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHotelsByUserId(int userId)
        {
            try
            {
                // Verify authorization
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!User.IsInRole("ADMIN") && (userIdClaim == null || userIdClaim != userId.ToString()))
                {
                    _logger.LogWarning("User {UserId} attempted to access hotels of another user: {TargetUserId}", userIdClaim, userId);
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new ApiResponse<List<HotelDTO>>(false, "You can only access your own hotels unless you are an Admin."));
                }

                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid user ID: {UserId}", userId);
                    return BadRequest(new ApiResponse<List<HotelDTO>>(false, "Invalid user ID."));
                }

                var response = await _hotelServices.GetHotelsByUserIdAsync(userId);
                if (!response.Success)
                {
                    _logger.LogWarning("Failed to retrieve hotels for UserId {UserId}: {Message}", userId, response.Message);
                    return NotFound(new ApiResponse<List<HotelDTO>>(false, response.Message));
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hotels for UserId: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<List<HotelDTO>>(false, $"Error retrieving hotels: {ex.Message}"));
            }
        }

        [HttpGet("{hotelId}/reservations")]
        [Authorize(Roles = "ADMIN,SERVICE_PROVIDER,ATTENDANT")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReservationsByHotelId(int hotelId)
        {
            try
            {
                if (hotelId <= 0)
                {
                    _logger.LogWarning("Invalid hotel ID: {HotelId}", hotelId);
                    return BadRequest(new ApiResponse<List<ReserveDTO>>(false, "Invalid hotel ID."));
                }

                // Authorization check for SERVICE_PROVIDER
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!User.IsInRole("ADMIN") && !User.IsInRole("ATTENDANT"))
                {
                    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    {
                        _logger.LogWarning("Invalid or missing UserId in claims");
                        return Unauthorized(new ApiResponse<List<ReserveDTO>>(false, "User not authenticated."));
                    }

                    var hotel = await _hotelServices.GetHotelByIdAsync(hotelId);
                    if (hotel.Data?.UserId != userId)
                    {
                        _logger.LogWarning("User {UserId} attempted to access reservations for hotel {HotelId} they do not own", userId, hotelId);
                        return StatusCode(StatusCodes.Status403Forbidden,
                            new ApiResponse<List<ReserveDTO>>(false, "You can only access reservations for your own hotels."));
                    }
                }

                var response = await _hotelServices.GetReservationsByHotelIdAsync(hotelId);
                if (!response.Success)
                {
                    _logger.LogWarning("Failed to retrieve reservations for HotelId {HotelId}: {Message}", hotelId, response.Message);
                    return NotFound(new ApiResponse<List<ReserveDTO>>(false, response.Message));
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservations for HotelId: {HotelId}", hotelId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<List<ReserveDTO>>(false, $"Error retrieving reservations: {ex.Message}"));
            }
        }
    }
}