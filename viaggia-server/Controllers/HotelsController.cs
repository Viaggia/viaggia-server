using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
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

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelController : ControllerBase
    {
        private readonly IRepository<Hotel> _genericRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IWebHostEnvironment _environment;

        public HotelController(
            IRepository<Hotel> genericRepository,
            IHotelRepository hotelRepository,
            IWebHostEnvironment environment)
        {
            _genericRepository = genericRepository;
            _hotelRepository = hotelRepository;
            _environment = environment;
        }

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
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateHotel([FromForm] CreateHotelDTO createHotelDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<CreateHotelDTO>(false, "Invalid data.", null, ModelState));

            try
            {
                // Validar arquivos de mídia
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                if (createHotelDto.MediaFiles != null)
                {
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
                };

                var createdHotel = await _genericRepository.AddAsync(hotel);

                // Adicionar datas
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
                        await _hotelRepository.AddHotelDateAsync(hotelDate);
                    }
                }

                // Adicionar tipos de quarto
                if (createHotelDto.RoomTypes != null)
                {
                    foreach (var rtDto in createHotelDto.RoomTypes)
                    {
                        var roomType = new HotelRoomType
                        {
                            Name = rtDto.Name,
                            Price = rtDto.Price,
                            Capacity = rtDto.Capacity,
                            BedType = rtDto.BedType,
                            IsActive = rtDto.IsActive,
                            HotelId = createdHotel.HotelId
                        };
                        await _hotelRepository.AddRoomTypeAsync(roomType);
                    }
                }

                // Adicionar pacotes
                if (createHotelDto.Packages != null)
                {
                    foreach (var packageDto in createHotelDto.Packages)
                    {
                        var package = new Package
                        {
                            Name = packageDto.Name,
                            Description = packageDto.Description,
                            BasePrice = packageDto.BasePrice,
                            IsActive = packageDto.IsActive,
                            HotelId = createdHotel.HotelId
                        };
                        await _hotelRepository.AddPackageAsync(package);
                    }
                }

                // Upload e salvar mídias
                var uploadPath = Path.Combine(_environment.WebRootPath, "Uploads", "Hotel");
                Directory.CreateDirectory(uploadPath);

                if (createHotelDto.MediaFiles != null)
                {
                    foreach (var file in createHotelDto.MediaFiles)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);
                        using var stream = new FileStream(filePath, FileMode.Create);
                        await file.CopyToAsync(stream);

                        var media = new Media
                        {
                            MediaUrl = $"/Uploads/Hotel/{fileName}",
                            MediaType = file.ContentType,
                            HotelId = createdHotel.HotelId
                        };
                        await _hotelRepository.AddMediaAsync(media);
                    }
                }

                // ✅ Buscar hotel com todos os relacionamentos preenchidos
                var hotelWithDetails = await _hotelRepository.GetHotelByIdWithDetailsAsync(createdHotel.HotelId);

                return CreatedAtAction(nameof(GetHotelById), new { id = createdHotel.HotelId },
                    new ApiResponse<Hotel>(true, "Hotel criado com sucesso.", hotelWithDetails));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<Hotel>(false, $"Erro ao criar hotel: {innerMessage}"));
            }
        }



        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] HotelDTO hotelDto)
        {
            if (!ModelState.IsValid || id != hotelDto.HotelId)
                return BadRequest(new ApiResponse<object>(false, "Dados inválidos."));

            var existingHotel = await _genericRepository.GetByIdAsync(id);
            if (existingHotel == null)
                return NotFound(new ApiResponse<object>(false, "Hotel não encontrado."));

            existingHotel.Name = hotelDto.Name;
            existingHotel.Description = hotelDto.Description;
            existingHotel.IsActive = hotelDto.IsActive;

            await _genericRepository.UpdateAsync(existingHotel);
            return Ok(new ApiResponse<object>(true, "Hotel atualizado com sucesso."));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var deleted = await _genericRepository.SoftDeleteAsync(id);
            if (deleted)
                return Ok(new ApiResponse<object>(true, "Hotel excluído com sucesso."));
            else
                return NotFound(new ApiResponse<object>(false, "Hotel não encontrado."));
        }
    }
}