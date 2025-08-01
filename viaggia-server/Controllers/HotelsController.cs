using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Commoditie;
using viaggia_server.DTOs.Hotel;
using viaggia_server.DTOs.Hotels;
using viaggia_server.DTOs.Packages;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.HotelDates;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Reviews;
using viaggia_server.Repositories.HotelRepository;
using viaggia_server.Services;
using viaggia_server.Services.HotelServices;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelsController : ControllerBase
    {
        private readonly IHotelServices _hotelServices;
        private readonly IWebHostEnvironment _environment;
        private readonly IHotelRepository _hotelRepository;

        public HotelsController(IHotelServices hotelServices,
                 IWebHostEnvironment environment,
                 IHotelRepository hotelRepository)
        {
            _hotelServices = hotelServices;
            _environment = environment;
            _hotelRepository = hotelRepository;
        }

        // GET: api/hotels
        [HttpGet]
        public async Task<IActionResult> GetAllHotelsAsync()
        {
            var response = await _hotelServices.GetAllHotelsAsync();
            if (response is ApiResponse<IEnumerable<Hotel>> apiResponse && apiResponse.Success)
            {
                return Ok(apiResponse);
            }
            return StatusCode(500, response);
        }

        // GET: api/hotels/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetHotelByIdAsync(int id)
        {
            var response = await _hotelServices.GetHotelByIdAsync(id);
            try
            {
                if (response == null)
                    return NotFound(new ApiResponse<object>(false, "Hotel não encontrado.", null));
                return Ok(new ApiResponse<Hotel>(true, "Hotel encontrado com sucesso.", response));

            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, "Erro ao buscar hotel.", null, ex.Message));
            }
            return Ok(response);
        }

        [HttpPost("create-json")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> CreateHotel([FromForm] CreateHotelDTO createHotelDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<object>(false, "Dados inválidos.", null, ModelState));

                // 1. Criar hotel
                var hotel = new Hotel
                {
                    Name = createHotelDto.Name,
                    Cnpj = createHotelDto.Cnpj,
                    Street = createHotelDto.Street,
                    City = createHotelDto.City,
                    State = createHotelDto.State,
                    ZipCode = createHotelDto.ZipCode,
                    Description = createHotelDto.Description,
                    StarRating = createHotelDto.StarRating,
                    CheckInTime = createHotelDto.CheckInTime,
                    CheckOutTime = createHotelDto.CheckOutTime,
                    ContactPhone = createHotelDto.ContactPhone
                };

                var response = await _hotelServices.AddHotelAsync(hotel);
                if (response == null)
                    return StatusCode(500, new ApiResponse<object>(false, "Erro ao criar hotel."));

                int hotelId = response.HotelId;

                // 2. HotelDates
                if (createHotelDto.HotelDates != null && createHotelDto.HotelDates.Any())
                {
                    foreach (var date in createHotelDto.HotelDates)
                    {
                        var hotelDate = new HotelDate
                        {
                            HotelId = hotelId,
                            StartDate = date.StartDate,
                            EndDate = date.EndDate,
                            AvailableRooms = date.AvailableRooms,
                            IsActive = date.IsActive
                        };
                        await _hotelRepository.AddHotelDateAsync(hotelDate);
                    }
                }

                // 3. RoomTypes
                if (createHotelDto.RoomTypes != null && createHotelDto.RoomTypes.Any())
                {
                    foreach (var room in createHotelDto.RoomTypes)
                    {
                        var roomType = new HotelRoomType
                        {
                            HotelId = hotelId,
                            Name = room.Name,
                            Description = room.Description,
                            Price = room.Price,
                            Capacity = room.Capacity,
                            BedType = room.BedType,
                            IsActive = room.IsActive
                        };
                        await _hotelRepository.AddRoomTypeAsync(roomType);
                    }
                }

                // 4. Packages
                if (createHotelDto.Packages != null && createHotelDto.Packages.Any())
                {
                    foreach (var pkg in createHotelDto.Packages)
                    {
                        var package = new Package
                        {
                            HotelId = hotelId,
                            Name = pkg.Name,
                            Description = pkg.Description,
                           
                            IsActive = pkg.IsActive
                        };
                        await _hotelRepository.AddPackageAsync(package);
                    }
                }

                // 5. Reviews
                if (createHotelDto.Reviews != null && createHotelDto.Reviews.Any())
                {
                    foreach (var review in createHotelDto.Reviews)
                    {
                        var reviewEntity = new Review
                        {
                            HotelId = hotelId,
                            Rating = review.Rating,
                            Comment = review.Comment,

                           
                        };
                        await _hotelRepository.AddReviewAsync(reviewEntity);
                    }
                }

                // 6. Commodities + CommoditieServices
                if (createHotelDto.Commodities != null && createHotelDto.Commodities.Any())
                {
                    foreach (var commoditieDto in createHotelDto.Commodities)
                    {
                        var commoditie = new Commoditie
                        {
                            HotelId = hotelId,
                            HasParking = commoditieDto.HasParking,
                            IsParkingPaid = commoditieDto.IsParkingPaid,
                            HasBreakfast = commoditieDto.HasBreakfast,
                            IsBreakfastPaid = commoditieDto.IsBreakfastPaid,
                            HasLunch = commoditieDto.HasLunch,
                            IsLunchPaid = commoditieDto.IsLunchPaid,
                            HasDinner = commoditieDto.HasDinner,
                            IsDinnerPaid = commoditieDto.IsDinnerPaid,
                            HasSpa = commoditieDto.HasSpa,
                            IsSpaPaid = commoditieDto.IsSpaPaid,
                            HasPool = commoditieDto.HasPool,
                            IsPoolPaid = commoditieDto.IsPoolPaid,
                            HasGym = commoditieDto.HasGym,
                            IsGymPaid = commoditieDto.IsGymPaid,
                            HasWiFi = commoditieDto.HasWiFi,
                            IsWiFiPaid = commoditieDto.IsWiFiPaid,
                            HasAirConditioning = commoditieDto.HasAirConditioning,
                            IsAirConditioningPaid = commoditieDto.IsAirConditioningPaid,
                            HasAccessibilityFeatures = commoditieDto.HasAccessibilityFeatures,
                            IsAccessibilityFeaturesPaid = commoditieDto.IsAccessibilityFeaturesPaid,
                            IsPetFriendly = commoditieDto.IsPetFriendly,
                            IsPetFriendlyPaid = commoditieDto.IsPetFriendlyPaid,
                            IsActive = commoditieDto.IsActive
                        };

                        // Adiciona a commoditie e salva no banco (gera ID)
                        await _hotelRepository.AddCommoditieAsync(commoditie);

                        // Adiciona os serviços personalizados relacionados
                        if (commoditieDto.CommoditieServices != null && commoditieDto.CommoditieServices.Any())
                        {
                            foreach (var serviceDto in commoditieDto.CommoditieServices)
                            {
                                var service = new CommoditieServices
                                {
                                    HotelId = hotelId,
                                    CommoditieId = commoditie.CommoditieId,
                                    Name = serviceDto.Name,
                                    Description = serviceDto.Description,
                                    IsPaid = serviceDto.IsPaid,
                                    IsActive = serviceDto.IsActive
                                };

                                await _hotelRepository.AddCommoditieServiceAsync(service);
                            }
                        }
                    }
                }

                // 7. Medias (arquivos)
                if (createHotelDto.MediaFiles != null && createHotelDto.MediaFiles.Any())
                {
                    var uploadPath = Path.Combine(_environment.WebRootPath, "Uploads", "Hotels");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    foreach (var file in createHotelDto.MediaFiles)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(uploadPath, fileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var media = new Media
                            {
                                HotelId = hotelId,
                                MediaUrl = $"/Uploads/Hotels/{fileName}",
                                MediaType = file.ContentType
                            };
                            await _hotelServices.AddMediaToHotelAsync(media);
                        }
                    }
                }

                return Created($"/api/hotels/{hotelId}", new ApiResponse<Hotel>(true, "Hotel criado com sucesso.", response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, "Erro ao criar hotel.", null, ex.Message));
            }
        }


        // POST: api/hotels/upload-media
        [HttpPost("upload-media")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateMedia(int id, [FromForm] HotelDTO hotelDTO)
        {
            if (hotelDTO == null || id != hotelDTO.HotelId || !ModelState.IsValid)
                return BadRequest(new ApiResponse<MediaDTO>(false, "Invalid media data or ID."));

            try
            {
                var hotel = await _hotelServices.GetHotelByIdAsync(id);
                if (hotel == null)
                    return NotFound(new ApiResponse<HotelDTO>(false, $"Hotel with ID {id} not found."));


                hotel.Medias = (await _hotelServices.GetMediaByHotelIdAsync(id)).ToList();
                
                var uploadPath = Path.Combine(_environment.WebRootPath, "Uploads", "Packages");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var file in hotelDTO.MediaFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        var media = new Media
                        {
                            MediaUrl = $"/Uploads/Packages/{fileName}",
                            MediaType = file.ContentType,
                            HotelId = hotel.HotelId
                        };
                        await _hotelServices.AddMediaToHotelAsync(media);
                        hotel.Medias.Add(media);
                    }
                }
                

                await _hotelServices.SaveChangesAsync();


                return Ok();

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<MediaDTO>(false, $"Error create media: {ex.Message}"));
            }
        }


        // PUT: api/hotels/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateHotelAsync(int id, [FromBody] UpdateHotelDto updateHotelDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>(false, "Dados inválidos.", null, ModelState));

            var response = await _hotelServices.UpdateHotelAsync(updateHotelDto);
            
               

            return BadRequest(response);
        }

        // DELETE: api/hotels/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteHotelAsync(int id)
        {
            var response = await _hotelServices.SoftDeleteHotelAsync(id);
         
            return NotFound(response);
        }
    }
}