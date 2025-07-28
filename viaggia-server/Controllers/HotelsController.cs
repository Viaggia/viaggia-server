/*using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Commodities;
using viaggia_server.DTOs.Hotels;
using viaggia_server.DTOs.Packages;
using viaggia_server.DTOs.Reviews;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Packages;
using viaggia_server.Repositories;
using viaggia_server.Repositories.Commodities;
using viaggia_server.Repositories.Hotel;
using viaggia_server.Services;
using viaggia_server.Services.Commodities; // Supondo que o CommoditieService está nesse namespace

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelController : ControllerBase
    {
        private readonly IRepository<Hotel> _genericRepository;
        private readonly IHotelRepository  _hotelRepository;// para RoomType e HotelDate
        private readonly ICommoditiesService _commoditieService;

        public HotelController(
            IRepository<Hotel> genericRepo,
            IHotelRepository hotelRepo,
            ICommoditiesService commoditieService)
        {
            _genericRepository = genericRepo;
            _hotelRepository = hotelRepo;
            _commoditieService = commoditieService;
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
                var hotelDTOs = new List<HotelDTO>();
                foreach (var h in hotels)
                {
                    var hotel = await _genericRepository.GetByIdAsync<Hotel>(h.HotelId);
                    hotelDTOs.Add(new HotelDTO
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
                        Commoditie = new CommoditieDTO
                        {
                            CommoditieId = h.Commoditie.CommoditieId,
                            HasParking = h.Commoditie.HasParking,
                            IsParkingPaid = h.Commoditie.IsParkingPaid,
                            HasBreakfast = h.Commoditie.HasBreakfast,
                            IsBreakfastPaid = h.Commoditie.IsBreakfastPaid,
                            HasLunch = h.Commoditie.HasLunch,
                            IsLunchPaid = h.Commoditie.IsLunchPaid,
                            HasDinner = h.Commoditie.HasDinner,
                            IsDinnerPaid = h.Commoditie.IsDinnerPaid,
                            HasSpa = h.Commoditie.HasSpa,
                            IsSpaPaid = h.Commoditie.IsSpaPaid,
                            HasPool = h.Commoditie.HasPool,
                            IsPoolPaid = h.Commoditie.IsPoolPaid,
                            HasGym = h.Commoditie.HasGym,
                            IsGymPaid = h.Commoditie.IsGymPaid,
                            HasWiFi = h.Commoditie.HasWiFi,
                            IsWiFiPaid = h.Commoditie.IsWiFiPaid,
                            HasAirConditioning = h.Commoditie.HasAirConditioning,
                            IsAirConditioningPaid = h.Commoditie.IsAirConditioningPaid,
                            HasAccessibilityFeatures = h.Commoditie.HasAccessibilityFeatures,
                            IsAccessibilityFeaturesPaid = h.Commoditie.IsAccessibilityFeaturesPaid,
                            IsPetFriendly = h.Commoditie.IsPetFriendly,
                            IsPetFriendlyPaid = h.Commoditie.IsPetFriendlyPaid
                        },
                        AverageRating = h.Reviews.Any() ? h.Reviews.Average(r => r.Rating) : 0

                    });
                }
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

                hotel.RoomTypes = (await _packageRepository.GetPackageMediasAsync(id)).ToList();
                hotel.HotelDates = (await _packageRepository.GetPackageDatesAsync(id)).ToList();
                hotel.Medias = (await _packageRepository.GetPackageMediasAsync(id)).ToList();
                hotel.Reviews = (await _packageRepository.GetPackageReviewsAsync(id)).ToList();
                hotel.Commoditie = await _commoditieService.GetByHotelIdAsync(id);
                hotel.Address = await _genericRepository.GetAddressByHotelIdAsync(id);
                hotel.AverageRating = hotel.Reviews.Any() ? hotel.Reviews.Average(r => r.Rating) : 0;

                var hotel = await _genericRepository.GetByIdAsync<Hotel>(package.HotelId);

                var packageDTO = new PackageDTO
                {
                    PackageId = package.PackageId,
                    Name = package.Name,
                    Destination = package.Destination,
                    Description = package.Description,
                    BasePrice = package.BasePrice,
                    HotelId = package.HotelId,
                    HotelName = hotel?.Name ?? string.Empty,
                    IsActive = package.IsActive,
                    Medias = package.Medias.Select(m => new MediaDTO
                    {
                        MediaId = m.MediaId,
                        MediaUrl = m.MediaUrl,
                        MediaType = m.MediaType
                    }).ToList(),
                    PackageDates = package.PackageDates.Select(pd => new PackageDateDTO
                    {
                        PackageDateId = pd.PackageDateId,
                        StartDate = pd.StartDate,
                        EndDate = pd.EndDate
                    }).ToList()
                };
                return Ok(new ApiResponse<PackageDTO>(true, "Package retrieved successfully.", packageDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<PackageDTO>(false, $"Error retrieving package: {ex.Message}"));
            }

            // POST: api/hotel
            [HttpPost]
             public async Task<IActionResult> CreateHotel([FromBody] CreateHotelDTO createHotelDto)
            {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var hotel = new Hotel
            {
                Name = createHotelDto.Name,
                // Mapear demais campos de endereço, descrição, contatos etc.
                IsActive = createHotelDto.IsActive,
                // Assumindo que o endereço seja um objeto relacionado, você pode criar isso e associar aqui
            };

            // Criar o hotel principal
            var createdHotel = await _hotelRepository.AddAsync(hotel);

            // Adicionar RoomTypes
            foreach (var rtDto in createHotelDto.RoomTypes)
            {
                var roomType = new HotelRoomType
                {
                    Name = rtDto.Name,
                    Price = rtDto.Price,
                    Capacity = rtDto.Capacity,
                    BedType = rtDto.BedType,
                    HotelId = createdHotel.HotelId,
                    IsActive = rtDto.IsActive
                };
                await _hotelRepository.AddRoomTypeAsync(roomType);
            }

            // Adicionar HotelDates (se existir)
            foreach (var hdDto in createHotelDto.HotelDates)
            {
                var hotelDate = new HotelDate
                {
                    StartDate = hdDto.StartDate,
                    EndDate = hdDto.EndDate,
                    AvailableRooms = hdDto.AvailableRooms,
                    HotelId = createdHotel.HotelId,
                    IsActive = hdDto.IsActive,
                    // Você pode ajustar RoomTypeId aqui se necessário
                };
                // Use repositório específico para HotelDate (não fornecido no exemplo, mas você pode criar)
                // await _hotelDateRepository.AddAsync(hotelDate);
                // Aqui só um exemplo de como pode ser
            }

            // Criar e associar Commoditie via serviço
            var commoditie = new Commoditie
            {
                HotelId = createdHotel.HotelId,
                HasParking = createHotelDto.Commoditie.HasParking,
                IsParkingPaid = createHotelDto.Commoditie.IsParkingPaid,
                HasBreakfast = createHotelDto.Commoditie.HasBreakfast,
                IsBreakfastPaid = createHotelDto.Commoditie.IsBreakfastPaid,
                // Mapear demais campos
            };
            await _commoditieService.AddAsync(commoditie);

            // Se houver upload de mídia, trate aqui (não implementado)

            return CreatedAtAction(nameof(GetHotelById), new { id = createdHotel.HotelId }, createdHotel);
        }

        // PUT: api/hotel/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] HotelDTO hotelDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != hotelDto.HotelId) return BadRequest("ID mismatch");

            var existingHotel = await _genericRepository.GetByIdAsync(id);
            if (existingHotel == null) return NotFound();

            // Atualizar campos simples
            existingHotel.Name = hotelDto.Name;
            existingHotel.Description = hotelDto.Description;
            existingHotel.IsActive = hotelDto.IsActive;

            await _genericRepository.UpdateAsync(existingHotel);

            // Atualizar RoomTypes e HotelDates também, se desejar
            // Atualizar Commoditie via serviço
            if (hotelDto.Commoditie != null)
            {
                var commoditie = await _commoditieService.GetByHotelIdAsync(id);
                if (commoditie != null)
                {
                    commoditie.HasParking = hotelDto.Commoditie.HasParking;
                    commoditie.IsParkingPaid = hotelDto.Commoditie.IsParkingPaid;
                    // Mapear os demais campos
                    await _commoditieService.UpdateAsync(commoditie);
                }
            }

            return NoContent();
        }

        // DELETE: api/hotel/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var deleted = await _genericRepository.SoftDeleteAsync(id);
            if (!deleted) return NotFound();

            // Pode chamar soft delete em commodities e outras entidades relacionadas, se desejar

            return NoContent();
        }
    }
}
*/