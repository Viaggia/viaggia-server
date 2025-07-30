using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Commoditie;
using viaggia_server.DTOs.Hotel;
using viaggia_server.DTOs.Hotels;
using viaggia_server.DTOs.Packages;
using viaggia_server.DTOs.Reviews;
using viaggia_server.Models.Addresses;
using viaggia_server.Models.Commodities;
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
        private readonly ILogger<HotelController> _logger;

        public HotelController(
            IRepository<Hotel> genericRepository,
            IHotelRepository hotelRepository,
            IWebHostEnvironment environment,
            ILogger<HotelController> logger)
        {
            _genericRepository = genericRepository;
            _hotelRepository = hotelRepository;
            _environment = environment;
            _logger = logger;
        }

        // GET: api/hotel
        [HttpGet]
        public async Task<IActionResult> GetAllHotels()
        {
            try
            {
                var hotels = await _genericRepository.GetAllAsync();
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
                        Street = hotel.Street,
                        City = hotel.City,
                        State = hotel.State,
                        ZipCode = hotel.ZipCode,

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
                var hotel = await _genericRepository.GetByIdAsync(id);
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
                        //IsActive = rt.IsActive
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
            {
                return BadRequest(new ApiResponse<CreateHotelDTO>(false, "Dados inválidos.", null, ModelState));
            }

            try
            {
                // Configuração robusta do JSON serializer
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };

                // Função auxiliar melhorada para desserialização
                List<T> DeserializeJsonList<T>(string json, string propertyName) where T : class
                {
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return new List<T>();
                    }

                    try
                    {
                        // Pré-processamento para remover possíveis escapes extras
                        var cleanJson = json
                            .Replace("\\\"", "\"")  // Remove escapes de aspas
                            .Trim();

                        // Verifica se começa com [ e termina com ] (array JSON válido)
                        if (!cleanJson.StartsWith("[") || !cleanJson.EndsWith("]"))
                        {
                            throw new JsonException($"O valor de {propertyName} deve ser um array JSON válido (começar com [ e terminar com ])");
                        }

                        return JsonSerializer.Deserialize<List<T>>(cleanJson, jsonOptions) ?? new List<T>();
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError($"Erro ao desserializar {propertyName}. Valor recebido: {json}");
                        throw new ArgumentException($"Formato inválido para {propertyName}. Deve ser um array JSON válido. Exemplo: [{{\"Street\":\"Rua Exemplo\",\"City\":\"São Paulo\"}}]");
                    }
                }

                // Desserialização com tratamento aprimorado
               
                var roomTypes = DeserializeJsonList<HotelRoomTypeDTO>(createHotelDto.RoomTypesJson, nameof(createHotelDto.RoomTypesJson));
                var hotelDates = DeserializeJsonList<HotelDateDTO>(createHotelDto.HotelDatesJson, nameof(createHotelDto.HotelDatesJson));
                var packages = DeserializeJsonList<PackageDTO>(createHotelDto.PackagesJson, nameof(createHotelDto.PackagesJson));

                // Validação de arquivos
                if (createHotelDto.MediaFiles != null)
                    {
                        foreach (var file in createHotelDto.MediaFiles)
                        {
                            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                            if (!new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(ext))
                            {
                                return BadRequest(new ApiResponse<object>(false, $"Tipo de arquivo não suportado: {ext}"));
                            }

                            if (file.Length > 5 * 1024 * 1024) // 5MB
                            {
                                return BadRequest(new ApiResponse<object>(false, $"Arquivo {file.FileName} excede o tamanho máximo de 5MB"));
                            }
                        }
                    }

                    // Criar hotel principal
                    var hotel = new Hotel
                    {
                        Name = createHotelDto.Name,
                        Cnpj = createHotelDto.Cnpj,
                        Description = createHotelDto.Description,
                        StarRating = createHotelDto.StarRating,
                        CheckInTime = createHotelDto.CheckInTime,
                        CheckOutTime = createHotelDto.CheckOutTime,
                        ContactPhone = createHotelDto.ContactPhone,
                        ContactEmail = createHotelDto.ContactEmail,
                        IsActive = createHotelDto.IsActive,
                        Street = createHotelDto.Street,
                        City = createHotelDto.City,
                        State = createHotelDto.State,
                        ZipCode = createHotelDto.ZipCode
                    };

                    // Salvar hotel primeiro para obter o ID
                    var createdHotel = await _genericRepository.AddAsync(hotel);

                    // Processar entidades relacionadas
                    await ProcessRelatedEntities(createdHotel.HotelId, roomTypes, hotelDates, packages, createHotelDto.MediaFiles);

                    return CreatedAtAction(nameof(GetHotelById), new { id = createdHotel.HotelId },
                        new ApiResponse<Hotel>(true, "Hotel criado com sucesso.", createdHotel));
                }
                catch (ValidationException valEx)
                {
                    return BadRequest(new ApiResponse<object>(false, valEx.Message));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao criar hotel");
                    return StatusCode(500, new ApiResponse<object>(false, "Ocorreu um erro interno ao processar sua requisição."));
                }
            }

            private async Task ProcessRelatedEntities(
                int hotelId,
                List<HotelRoomTypeDTO> roomTypes,
                List<HotelDateDTO> hotelDates,
                List<PackageDTO> packages,
                List<IFormFile> mediaFiles)
            {
                // Adicionar tipos de quarto
                foreach (var rtDto in roomTypes)
                {
                    await _hotelRepository.AddRoomTypeAsync(new HotelRoomType
                    {
                        Name = rtDto.Name,
                        Price = rtDto.Price,
                        Capacity = rtDto.Capacity,
                        BedType = rtDto.BedType,
                        IsActive = rtDto.IsActive,
                        HotelId = hotelId
                    });
                }

                // Adicionar datas do hotel
                foreach (var hdDto in hotelDates)
                {
                    await _hotelRepository.AddHotelDateAsync(new HotelDate
                    {
                        StartDate = hdDto.StartDate,
                        EndDate = hdDto.EndDate,
                        AvailableRooms = hdDto.AvailableRooms,
                        IsActive = hdDto.IsActive,
                        HotelId = hotelId
                    });
                }

                // Adicionar pacotes
                foreach (var pkgDto in packages)
                {
                    await _hotelRepository.AddPackageAsync(new Package
                    {
                        Name = pkgDto.Name,
                        Description = pkgDto.Description,
                        BasePrice = pkgDto.BasePrice,
                        IsActive = pkgDto.IsActive,
                        HotelId = hotelId
                    });
                }

                // Processar arquivos de mídia
                if (mediaFiles != null && mediaFiles.Any())
                {
                    var uploadPath = Path.Combine(_environment.WebRootPath, "Uploads", "Hotels");
                    Directory.CreateDirectory(uploadPath);

                    foreach (var file in mediaFiles)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        await _hotelRepository.AddMediaAsync(new Media
                        {
                            MediaUrl = $"/Uploads/Hotels/{fileName}",
                            MediaType = file.ContentType,
                            HotelId = hotelId
                        });
                    }
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