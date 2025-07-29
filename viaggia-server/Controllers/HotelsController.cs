using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Commoditie;
using viaggia_server.DTOs.Commodity;
using viaggia_server.DTOs.Hotels;
using viaggia_server.DTOs.Packages;
using viaggia_server.DTOs.Reviews;
using viaggia_server.Models.Addresses;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.HotelDates;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Repositories;
using viaggia_server.Repositories.Commodities;
using viaggia_server.Repositories.HotelRepository;
using viaggia_server.Data;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelController : ControllerBase
    {
        private readonly IRepository<Hotel> _genericRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly ICommoditieRepository _commoditieRepository;
        private readonly AppDbContext _context;

        public HotelController(
            IRepository<Hotel> genericRepository,
            IHotelRepository hotelRepository,
            IWebHostEnvironment environment,
            ICommoditieRepository commoditieRepository,
            AppDbContext context)
        {
            _genericRepository = genericRepository;
            _hotelRepository = hotelRepository;
            _environment = environment;
            _commoditieRepository = commoditieRepository;
            _context = context;
        }

        // GET: api/hotel
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllHotels()
        {
            try
            {
                var hotels = await _genericRepository.GetAllAsync();
                var hotelDTOs = hotels.Select(h => new HotelDTO
                {
                    HotelId = h.HotelId,
                    Name = h.Name,
                    Description = h.Description,
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
                var hotel = await _genericRepository.GetByIdAsync(id);
                if (hotel == null)
                    return NotFound(new ApiResponse<HotelDTO>(false, $"Hotel with ID {id} not found."));

                hotel.RoomTypes = (await _hotelRepository.GetHotelRoomTypesAsync(id)).ToList();
                hotel.HotelDates = (await _hotelRepository.GetHotelDatesAsync(id)).ToList();
                hotel.Medias = (await _hotelRepository.GetMediasByHotelIdAsync(id)).ToList();
                hotel.Reviews = (await _hotelRepository.GetReviewsByHotelIdAsync(id)).ToList();
                hotel.Address = await _hotelRepository.GetAddressByHotelIdAsync(id);
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

        // Atualize o método CreateHotel para usar a propriedade MediaFiles do CreateHotelDTO
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateHotel([FromForm] CreateHotelDTO createHotelDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<CreateHotelDTO>(false, "Invalid data.", null, ModelState));

            try
            {
                // Validate file types and sizes
                if (createHotelDto.MediaFiles != null && createHotelDto.MediaFiles.Any())
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    foreach (var file in createHotelDto.MediaFiles)
                    {
                        if (file.Length > 5 * 1024 * 1024) // 5MB limit
                            return BadRequest(new ApiResponse<CreateHotelDTO>(false, "File size exceeds 5MB."));
                        var extension = Path.GetExtension(file.FileName).ToLower();
                        if (!allowedExtensions.Contains(extension))
                            return BadRequest(new ApiResponse<CreateHotelDTO>(false, $"Invalid file type: {extension}. Allowed types: {string.Join(", ", allowedExtensions)}"));
                    }
                }

                // Primeiro, criar o endereço
                var address = new Address
                {
                    Street = createHotelDto.Street,
                    City = createHotelDto.City,
                    State = createHotelDto.State,
                    ZipCode = createHotelDto.ZipCode,
                    IsActive = true
                };

                var createdAddress = await _context.Addresses.AddAsync(address);
                await _context.SaveChangesAsync();

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
                    Cnpj = createHotelDto.Cnpj,
                    AddressId = createdAddress.Entity.AddressId
                };

                var createdHotel = await _genericRepository.AddAsync(hotel);

                // Add HotelDates
                if (!string.IsNullOrEmpty(createHotelDto.HotelDatesJson))
                {
                    var hotelDates = System.Text.Json.JsonSerializer.Deserialize<List<HotelDateDTO>>(createHotelDto.HotelDatesJson);
                    if (hotelDates != null)
                    {
                        foreach (var hdDto in hotelDates)
                        {
                            var hotelDate = new HotelDate
                            {
                                StartDate = hdDto.StartDate,
                                EndDate = hdDto.EndDate,
                                AvailableRooms = hdDto.AvailableRooms,
                                HotelId = createdHotel.HotelId,
                                IsActive = hdDto.IsActive
                            };
                            await _hotelRepository.AddHotelDateAsync(hotelDate);
                        }
                    }
                }

                
                // Process file uploads
                var uploadPath = Path.Combine(_environment.WebRootPath, "Uploads", "Hotel");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                if (createHotelDto.MediaFiles != null && createHotelDto.MediaFiles.Any())
                {
                    foreach (var file in createHotelDto.MediaFiles)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                            var filePath = Path.Combine(uploadPath, fileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            var media = new Media
                            {
                                MediaUrl = $"/Uploads/Hotel/{fileName}",
                                MediaType = file.ContentType,
                                HotelId = createdHotel.HotelId
                            };
                            await _hotelRepository.AddMediaAsync(media);
                        }
                    }
                }

                return Ok(new { 
                    success = true,
                    message = "Hotel created successfully.",
                    hotelId = createdHotel.HotelId,
                    name = createdHotel.Name,
                    addressId = createdHotel.AddressId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = $"Error creating hotel: {ex.Message}" });
            }
        }


        // PUT: api/hotel/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] HotelDTO hotelDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != hotelDto.HotelId) return BadRequest("ID mismatch");

            var existingHotel = await _genericRepository.GetByIdAsync(id);
            if (existingHotel == null) return NotFound();

            existingHotel.Name = hotelDto.Name;
            existingHotel.Description = hotelDto.Description;
            existingHotel.IsActive = hotelDto.IsActive;

            await _genericRepository.UpdateAsync(existingHotel);

            // Atualizar RoomTypes e HotelDates também, se desejar

            return NoContent();
        }

        // DELETE: api/hotel/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var deleted = await _genericRepository.SoftDeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }

        // Commodities endpoints

        // GET: api/hotel/{hotelId}/commodities
        [HttpGet("{hotelId:int}/commodities")]
        public async Task<IActionResult> GetCommoditiesByHotel(int hotelId)
        {
            var commodities = await _hotelRepository.GetCommoditiesByHotelIdAsync(hotelId);
            if (commodities == null || !commodities.Any())
                return NotFound(new ApiResponse<List<Commoditie>>(false, "Nenhuma commodity encontrada para este hotel."));
            return Ok(new ApiResponse<IEnumerable<Commoditie>>(true, "Commodities encontradas.", commodities));
        }

        // GET: api/hotel/commodities/{id}
        [HttpGet("commodities/{id:int}")]
        public async Task<IActionResult> GetCommoditieById(int id)
        {
            var commodity = await _hotelRepository.GetCommoditieByIdAsync(id);
            if (commodity == null)
                return NotFound(new ApiResponse<Commoditie>(false, "Commodity não encontrada."));
            return Ok(new ApiResponse<Commoditie>(true, "Commodity encontrada.", commodity));
        }

        // POST: api/hotel/{hotelId}/commodities
        [HttpPost("{hotelId:int}/commodities")]
        public async Task<IActionResult> AddCommoditie(int hotelId, [FromBody] CommoditieDTO commoditieDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var commoditie = new Commoditie
            {
                HotelId = hotelId,
                HasParking = commoditieDto.HasParking,
                HasBreakfast = commoditieDto.HasBreakfast,
                HasLunch = commoditieDto.HasLunch,
                HasDinner = commoditieDto.HasDinner,
                HasSpa = commoditieDto.HasSpa,
                HasPool = commoditieDto.HasPool,
                HasGym = commoditieDto.HasGym,
                IsActive = true
            };

            var created = await _hotelRepository.AddCommoditieAsync(commoditie);
            return CreatedAtAction(nameof(GetCommoditieById), new { id = created.CommoditieId }, new ApiResponse<Commoditie>(true, "Commodity criada.", created));
        }

        // PUT: api/hotel/commodities/{id}
        [HttpPut("commodities/{id:int}")]
        public async Task<IActionResult> UpdateCommoditie(int id, [FromBody] CommoditieDTO commoditieDto)
        {
            var existing = await _hotelRepository.GetCommoditieByIdAsync(id);
            if (existing == null)
                return NotFound();

            existing.HasParking = commoditieDto.HasParking;
            existing.HasBreakfast = commoditieDto.HasBreakfast;
            existing.HasLunch = commoditieDto.HasLunch;
            existing.HasDinner = commoditieDto.HasDinner;
            existing.HasSpa = commoditieDto.HasSpa;
            existing.HasPool = commoditieDto.HasPool;
            existing.HasGym = commoditieDto.HasGym;
            existing.IsActive = commoditieDto.IsActive;

            await _commoditieRepository.UpdateAsync(existing);
            return NoContent();
        }

        // DELETE: api/hotel/commodities/{id}
        [HttpDelete("commodities/{id:int}")]
        public async Task<IActionResult> DeleteCommoditie(int id)
        {
            var deleted = await _genericRepository.SoftDeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        // CommoditieServices endpoints

        // GET: api/hotel/{hotelId}/commoditie-services
        [HttpGet("{hotelId:int}/commoditie-services")]
        public async Task<IActionResult> GetCommoditieServicesByHotel(int hotelId)
        {
            var services = await _hotelRepository.GetCommoditieServicesByHotelIdAsync(hotelId);
            if (services == null || !services.Any())
                return NotFound(new ApiResponse<List<CommoditieServices>>(false, "Nenhum serviço personalizado encontrado."));
            return Ok(new ApiResponse<IEnumerable<CommoditieServices>>(true, "Serviços personalizados encontrados.", services));
        }

        // GET: api/hotel/commoditie-services/{id}
        [HttpGet("commoditie-services/{id:int}")]
        public async Task<IActionResult> GetCommoditieServiceById(int id)
        {
            var service = await _hotelRepository.GetCommoditieServiceByIdAsync(id);
            if (service == null)
                return NotFound(new ApiResponse<CommoditieServices>(false, "Serviço personalizado não encontrado."));
            return Ok(new ApiResponse<CommoditieServices>(true, "Serviço personalizado encontrado.", service));
        }

        // POST: api/hotel/{hotelId}/commoditie-services
        [HttpPost("{hotelId:int}/commoditie-services")]
        public async Task<IActionResult> AddCommoditieService(int hotelId, [FromBody] CommoditieServicesDTO serviceDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = new CommoditieServices
            {
                HotelId = hotelId,
                Name = serviceDto.Name,
                IsPaid = serviceDto.IsPaid,
                IsActive = true
            };

            var created = await _hotelRepository.AddCommoditieServiceAsync(service);
            return CreatedAtAction(nameof(GetCommoditieServiceById), new { id = created.CommoditieServicesId }, new ApiResponse<CommoditieServices>(true, "Serviço personalizado criado.", created));
        }

        // PUT: api/hotel/commoditie-services/{id}
        [HttpPut("commoditie-services/{id:int}")]
        public async Task<IActionResult> UpdateCommoditieService(int id, [FromBody] CommoditieServicesDTO serviceDto)
        {
            var existing = await _hotelRepository.GetCommoditieServiceByIdAsync(id);
            if (existing == null)
                return NotFound();

            existing.Name = serviceDto.Name;
            existing.IsPaid = serviceDto.IsPaid;
            existing.IsActive = serviceDto.IsActive;

            // For now, return the updated entity without persisting
            // TODO: Implement proper update method in CommoditieServices repository
            return Ok(existing);
        }

        // DELETE: api/hotel/commoditie-services/{id}
        [HttpDelete("commoditie-services/{id:int}")]
        public async Task<IActionResult> DeleteCommoditieService(int id)
        {
            var deleted = await _genericRepository.SoftDeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
    
}
    