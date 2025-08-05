using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Commodity;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.CustomCommodities;
using viaggia_server.Models.Hotels;
using viaggia_server.Repositories;
using viaggia_server.Repositories.CommodityRepository;
using viaggia_server.Repositories.HotelRepository;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommodityController : ControllerBase
    {
        private readonly ICommodityRepository _commodityRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IRepository<Hotel> _genericRepository;

        public CommodityController(
            ICommodityRepository commodityRepository,
            IHotelRepository hotelRepository,
            IRepository<Hotel> genericRepository)
        {
            _commodityRepository = commodityRepository;
            _hotelRepository = hotelRepository;
            _genericRepository = genericRepository;
        }

        // GET: api/Commodity
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var commodities = await _commodityRepository.GetAllAsync();
            var response = commodities.Select(c => new CommodityReadDTO
            {
                CommodityId = c.CommodityId,
                HotelId = c.HotelId,
                HotelName = c.Hotel?.Name ?? string.Empty,
                HasParking = c.HasParking,
                IsParkingPaid = c.IsParkingPaid,
                ParkingPrice = c.ParkingPrice,
                HasBreakfast = c.HasBreakfast,
                IsBreakfastPaid = c.IsBreakfastPaid,
                BreakfastPrice = c.BreakfastPrice,
                HasLunch = c.HasLunch,
                IsLunchPaid = c.IsLunchPaid,
                LunchPrice = c.LunchPrice,
                HasDinner = c.HasDinner,
                IsDinnerPaid = c.IsDinnerPaid,
                DinnerPrice = c.DinnerPrice,
                HasSpa = c.HasSpa,
                IsSpaPaid = c.IsSpaPaid,
                SpaPrice = c.SpaPrice,
                HasPool = c.HasPool,
                IsPoolPaid = c.IsPoolPaid,
                PoolPrice = c.PoolPrice,
                HasGym = c.HasGym,
                IsGymPaid = c.IsGymPaid,
                GymPrice = c.GymPrice,
                HasWiFi = c.HasWiFi,
                IsWiFiPaid = c.IsWiFiPaid,
                WiFiPrice = c.WiFiPrice,
                HasAirConditioning = c.HasAirConditioning,
                IsAirConditioningPaid = c.IsAirConditioningPaid,
                AirConditioningPrice = c.AirConditioningPrice,
                HasAccessibilityFeatures = c.HasAccessibilityFeatures,
                IsAccessibilityFeaturesPaid = c.IsAccessibilityFeaturesPaid,
                AccessibilityFeaturesPrice = c.AccessibilityFeaturesPrice,
                IsPetFriendly = c.IsPetFriendly,
                IsPetFriendlyPaid = c.IsPetFriendlyPaid,
                PetFriendlyPrice = c.PetFriendlyPrice,
                IsActive = c.IsActive,
                CustomCommodities = c.CustomCommodities.Select(cs => new CustomCommodityDTO
                {
                    CustomCommodityId = cs.CustomCommodityId,
                    Name = cs.Name,
                    Description = cs.Description,
                    IsPaid = cs.IsPaid,
                    Price = cs.Price,
                    IsActive = cs.IsActive,
                    HotelName = c.Hotel?.Name ?? string.Empty,
                    HotelId = cs.HotelId,
                    CommodityId = cs.CommodityId
                }).ToList()
            });
            return Ok(new ApiResponse<IEnumerable<CommodityReadDTO>>(true, "Commodities recuperadas com sucesso.", response));
        }

        // GET: api/Commodity/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var commodity = await _commodityRepository.GetByIdAsync(id);
            if (commodity == null)
                return NotFound(new ApiResponse<Commodity>(false, "Commodity não encontrada."));

            var response = new CommodityReadDTO
            {
                CommodityId = commodity.CommodityId,
                HotelId = commodity.HotelId,
                HotelName = commodity.Hotel?.Name ?? string.Empty,
                HasParking = commodity.HasParking,
                IsParkingPaid = commodity.IsParkingPaid,
                ParkingPrice = commodity.ParkingPrice,
                HasBreakfast = commodity.HasBreakfast,
                IsBreakfastPaid = commodity.IsBreakfastPaid,
                BreakfastPrice = commodity.BreakfastPrice,
                HasLunch = commodity.HasLunch,
                IsLunchPaid = commodity.IsLunchPaid,
                LunchPrice = commodity.LunchPrice,
                HasDinner = commodity.HasDinner,
                IsDinnerPaid = commodity.IsDinnerPaid,
                DinnerPrice = commodity.DinnerPrice,
                HasSpa = commodity.HasSpa,
                IsSpaPaid = commodity.IsSpaPaid,
                SpaPrice = commodity.SpaPrice,
                HasPool = commodity.HasPool,
                IsPoolPaid = commodity.IsPoolPaid,
                PoolPrice = commodity.PoolPrice,
                HasGym = commodity.HasGym,
                IsGymPaid = commodity.IsGymPaid,
                GymPrice = commodity.GymPrice,
                HasWiFi = commodity.HasWiFi,
                IsWiFiPaid = commodity.IsWiFiPaid,
                WiFiPrice = commodity.WiFiPrice,
                HasAirConditioning = commodity.HasAirConditioning,
                IsAirConditioningPaid = commodity.IsAirConditioningPaid,
                AirConditioningPrice = commodity.AirConditioningPrice,
                HasAccessibilityFeatures = commodity.HasAccessibilityFeatures,
                IsAccessibilityFeaturesPaid = commodity.IsAccessibilityFeaturesPaid,
                AccessibilityFeaturesPrice = commodity.AccessibilityFeaturesPrice,
                IsPetFriendly = commodity.IsPetFriendly,
                IsPetFriendlyPaid = commodity.IsPetFriendlyPaid,
                PetFriendlyPrice = commodity.PetFriendlyPrice,
                IsActive = commodity.IsActive,
                CustomCommodities = commodity.CustomCommodities.Select(cs => new CustomCommodityDTO
                {
                    CustomCommodityId = cs.CustomCommodityId,
                    Name = cs.Name,
                    Description = cs.Description,
                    IsPaid = cs.IsPaid,
                    Price = cs.Price,
                    IsActive = cs.IsActive,
                    HotelName = commodity.Hotel?.Name ?? string.Empty,
                    HotelId = cs.HotelId,
                    CommodityId = cs.CommodityId
                }).ToList()
            };
            return Ok(new ApiResponse<CommodityReadDTO>(true, "Commodity encontrada.", response));
        }

        // GET: api/Commodity/hotel/{hotelId}
        [HttpGet("hotel/{hotelId:int}")]
        public async Task<IActionResult> GetByHotelId(int hotelId)
        {
            var commodity = await _commodityRepository.GetByHotelIdAsync(hotelId);
            if (commodity == null)
                return NotFound(new ApiResponse<Commodity>(false, "Commodity não encontrada para este hotel."));

            var response = new CommodityReadDTO
            {
                CommodityId = commodity.CommodityId,
                HotelId = commodity.HotelId,
                HotelName = commodity.Hotel?.Name ?? string.Empty,
                HasParking = commodity.HasParking,
                IsParkingPaid = commodity.IsParkingPaid,
                ParkingPrice = commodity.ParkingPrice,
                HasBreakfast = commodity.HasBreakfast,
                IsBreakfastPaid = commodity.IsBreakfastPaid,
                BreakfastPrice = commodity.BreakfastPrice,
                HasLunch = commodity.HasLunch,
                IsLunchPaid = commodity.IsLunchPaid,
                LunchPrice = commodity.LunchPrice,
                HasDinner = commodity.HasDinner,
                IsDinnerPaid = commodity.IsDinnerPaid,
                DinnerPrice = commodity.DinnerPrice,
                HasSpa = commodity.HasSpa,
                IsSpaPaid = commodity.IsSpaPaid,
                SpaPrice = commodity.SpaPrice,
                HasPool = commodity.HasPool,
                IsPoolPaid = commodity.IsPoolPaid,
                PoolPrice = commodity.PoolPrice,
                HasGym = commodity.HasGym,
                IsGymPaid = commodity.IsGymPaid,
                GymPrice = commodity.GymPrice,
                HasWiFi = commodity.HasWiFi,
                IsWiFiPaid = commodity.IsWiFiPaid,
                WiFiPrice = commodity.WiFiPrice,
                HasAirConditioning = commodity.HasAirConditioning,
                IsAirConditioningPaid = commodity.IsAirConditioningPaid,
                AirConditioningPrice = commodity.AirConditioningPrice,
                HasAccessibilityFeatures = commodity.HasAccessibilityFeatures,
                IsAccessibilityFeaturesPaid = commodity.IsAccessibilityFeaturesPaid,
                AccessibilityFeaturesPrice = commodity.AccessibilityFeaturesPrice,
                IsPetFriendly = commodity.IsPetFriendly,
                IsPetFriendlyPaid = commodity.IsPetFriendlyPaid,
                PetFriendlyPrice = commodity.PetFriendlyPrice,
                IsActive = commodity.IsActive,
                CustomCommodities = commodity.CustomCommodities.Select(cs => new CustomCommodityDTO
                {
                    CustomCommodityId = cs.CustomCommodityId,
                    Name = cs.Name,
                    Description = cs.Description,
                    IsPaid = cs.IsPaid,
                    Price = cs.Price,
                    IsActive = cs.IsActive,
                    HotelName = commodity.Hotel?.Name ?? string.Empty,
                    HotelId = cs.HotelId,
                    CommodityId = cs.CommodityId
                }).ToList()
            };
            return Ok(new ApiResponse<CommodityReadDTO>(true, "Commodity do hotel encontrada.", response));
        }

        // GET: api/Commodity/list-hotels
        [HttpGet("list-hotels")]
        public async Task<IActionResult> GetHotelsList()
        {
            var hotels = await _genericRepository.GetAllAsync();
            var list = hotels
                .Where(h => h.IsActive)
                .Select(h => new { h.HotelId, h.Name })
                .OrderBy(h => h.Name)
                .ToList();

            if (!list.Any())
                return NotFound(new ApiResponse<object>(false, "Nenhum hotel ativo encontrado."));

            return Ok(new ApiResponse<object>(true, "Lista de hotéis recuperada com sucesso.", list));
        }

        // POST: api/Commodity
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateCommodityDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<CreateCommodityDTO>(false, "Dados inválidos.", null, ModelState));

            var hotel = await _hotelRepository.GetHotelByNameAsync(dto.HotelName);
            if (hotel == null)
                return BadRequest(new ApiResponse<object>(false, $"Hotel com nome '{dto.HotelName}' não encontrado."));

            var commodity = new Commodity
            {
                HotelId = hotel.HotelId,
                HasParking = dto.HasParking,
                IsParkingPaid = dto.IsParkingPaid,
                ParkingPrice = dto.ParkingPrice,
                HasBreakfast = dto.HasBreakfast,
                IsBreakfastPaid = dto.IsBreakfastPaid,
                BreakfastPrice = dto.BreakfastPrice,
                HasLunch = dto.HasLunch,
                IsLunchPaid = dto.IsLunchPaid,
                LunchPrice = dto.LunchPrice,
                HasDinner = dto.HasDinner,
                IsDinnerPaid = dto.IsDinnerPaid,
                DinnerPrice = dto.DinnerPrice,
                HasSpa = dto.HasSpa,
                IsSpaPaid = dto.IsSpaPaid,
                SpaPrice = dto.SpaPrice,
                HasPool = dto.HasPool,
                IsPoolPaid = dto.IsPoolPaid,
                PoolPrice = dto.PoolPrice,
                HasGym = dto.HasGym,
                IsGymPaid = dto.IsGymPaid,
                GymPrice = dto.GymPrice,
                HasWiFi = dto.HasWiFi,
                IsWiFiPaid = dto.IsWiFiPaid,
                WiFiPrice = dto.WiFiPrice,
                HasAirConditioning = dto.HasAirConditioning,
                IsAirConditioningPaid = dto.IsAirConditioningPaid,
                AirConditioningPrice = dto.AirConditioningPrice,
                HasAccessibilityFeatures = dto.HasAccessibilityFeatures,
                IsAccessibilityFeaturesPaid = dto.IsAccessibilityFeaturesPaid,
                AccessibilityFeaturesPrice = dto.AccessibilityFeaturesPrice,
                IsPetFriendly = dto.IsPetFriendly,
                IsPetFriendlyPaid = dto.IsPetFriendlyPaid,
                PetFriendlyPrice = dto.PetFriendlyPrice,
                IsActive = dto.IsActive,
                CustomCommodities = dto.CustomCommodities?.Select(cs => new CustomCommodity
                {
                    Name = cs.Name,
                    Description = cs.Description,
                    IsPaid = cs.IsPaid,
                    Price = (decimal)cs.Price,
                    IsActive = cs.IsActive,
                    HotelId = hotel.HotelId
                }).ToList() ?? new List<CustomCommodity>()
            };

            var result = await _commodityRepository.AddAsync(commodity);

            foreach (var customCommodity in commodity.CustomCommodities)
            {
                customCommodity.CommodityId = result.CommodityId;
            }
            await _genericRepository.SaveChangesAsync(); 

            var reloadedCommodity = await _commodityRepository.GetByIdAsync(result.CommodityId);
            if (reloadedCommodity == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>(false, "Erro ao recuperar a commodity após criação."));

            var response = new CommodityReadDTO
            {
                CommodityId = reloadedCommodity.CommodityId,
                HotelId = reloadedCommodity.HotelId,
                HotelName = hotel.Name,
                HasParking = reloadedCommodity.HasParking,
                IsParkingPaid = reloadedCommodity.IsParkingPaid,
                ParkingPrice = reloadedCommodity.ParkingPrice,
                HasBreakfast = reloadedCommodity.HasBreakfast,
                IsBreakfastPaid = reloadedCommodity.IsBreakfastPaid,
                BreakfastPrice = reloadedCommodity.BreakfastPrice,
                HasLunch = reloadedCommodity.HasLunch,
                IsLunchPaid = reloadedCommodity.IsLunchPaid,
                LunchPrice = reloadedCommodity.LunchPrice,
                HasDinner = reloadedCommodity.HasDinner,
                IsDinnerPaid = reloadedCommodity.IsDinnerPaid,
                DinnerPrice = reloadedCommodity.DinnerPrice,
                HasSpa = reloadedCommodity.HasSpa,
                IsSpaPaid = reloadedCommodity.IsSpaPaid,
                SpaPrice = reloadedCommodity.SpaPrice,
                HasPool = reloadedCommodity.HasPool,
                IsPoolPaid = reloadedCommodity.IsPoolPaid,
                PoolPrice = reloadedCommodity.PoolPrice,
                HasGym = reloadedCommodity.HasGym,
                IsGymPaid = reloadedCommodity.IsGymPaid,
                GymPrice = reloadedCommodity.GymPrice,
                HasWiFi = reloadedCommodity.HasWiFi,
                IsWiFiPaid = reloadedCommodity.IsWiFiPaid,
                WiFiPrice = reloadedCommodity.WiFiPrice,
                HasAirConditioning = reloadedCommodity.HasAirConditioning,
                IsAirConditioningPaid = reloadedCommodity.IsAirConditioningPaid,
                AirConditioningPrice = reloadedCommodity.AirConditioningPrice,
                HasAccessibilityFeatures = reloadedCommodity.HasAccessibilityFeatures,
                IsAccessibilityFeaturesPaid = reloadedCommodity.IsAccessibilityFeaturesPaid,
                AccessibilityFeaturesPrice = reloadedCommodity.AccessibilityFeaturesPrice,
                IsPetFriendly = reloadedCommodity.IsPetFriendly,
                IsPetFriendlyPaid = reloadedCommodity.IsPetFriendlyPaid,
                PetFriendlyPrice = reloadedCommodity.PetFriendlyPrice,
                IsActive = reloadedCommodity.IsActive,
                CustomCommodities = reloadedCommodity.CustomCommodities.Select(cs => new CustomCommodityDTO
                {
                    CustomCommodityId = cs.CustomCommodityId,
                    Name = cs.Name,
                    Description = cs.Description,
                    IsPaid = cs.IsPaid,
                    Price = cs.Price,
                    IsActive = cs.IsActive,
                    HotelName = hotel.Name,
                    HotelId = cs.HotelId,
                    CommodityId = cs.CommodityId
                }).ToList()
            };

            return Ok(new ApiResponse<CommodityReadDTO>(true, "Commodity criada com sucesso.", response));
        }

        // PUT: api/Commodity/{id}
        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateCommodityDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<UpdateCommodityDTO>(false, "Dados inválidos.", null, ModelState));

            var existing = await _commodityRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new ApiResponse<Commodity>(false, "Commodity não encontrada."));

            var hotel = await _hotelRepository.GetHotelByNameAsync(dto.HotelName);
            if (hotel == null)
                return BadRequest(new ApiResponse<object>(false, $"Hotel com nome '{dto.HotelName}' não encontrado."));

            // Atualiza os dados da commodity
            existing.HotelId = hotel.HotelId;
            existing.HasParking = dto.HasParking;
            existing.IsParkingPaid = dto.IsParkingPaid;
            existing.ParkingPrice = dto.ParkingPrice;
            existing.HasBreakfast = dto.HasBreakfast;
            existing.IsBreakfastPaid = dto.IsBreakfastPaid;
            existing.BreakfastPrice = dto.BreakfastPrice;
            existing.HasLunch = dto.HasLunch;
            existing.IsLunchPaid = dto.IsLunchPaid;
            existing.LunchPrice = dto.LunchPrice;
            existing.HasDinner = dto.HasDinner;
            existing.IsDinnerPaid = dto.IsDinnerPaid;
            existing.DinnerPrice = dto.DinnerPrice;
            existing.HasSpa = dto.HasSpa;
            existing.IsSpaPaid = dto.IsSpaPaid;
            existing.SpaPrice = dto.SpaPrice;
            existing.HasPool = dto.HasPool;
            existing.IsPoolPaid = dto.IsPoolPaid;
            existing.PoolPrice = dto.PoolPrice;
            existing.HasGym = dto.HasGym;
            existing.IsGymPaid = dto.IsGymPaid;
            existing.GymPrice = dto.GymPrice;
            existing.HasWiFi = dto.HasWiFi;
            existing.IsWiFiPaid = dto.IsWiFiPaid;
            existing.WiFiPrice = dto.WiFiPrice;
            existing.HasAirConditioning = dto.HasAirConditioning;
            existing.IsAirConditioningPaid = dto.IsAirConditioningPaid;
            existing.AirConditioningPrice = dto.AirConditioningPrice;
            existing.HasAccessibilityFeatures = dto.HasAccessibilityFeatures;
            existing.IsAccessibilityFeaturesPaid = dto.IsAccessibilityFeaturesPaid;
            existing.AccessibilityFeaturesPrice = dto.AccessibilityFeaturesPrice;
            existing.IsPetFriendly = dto.IsPetFriendly;
            existing.IsPetFriendlyPaid = dto.IsPetFriendlyPaid;
            existing.PetFriendlyPrice = dto.PetFriendlyPrice;
            existing.IsActive = dto.IsActive;

            await _commodityRepository.UpdateAsync(existing);

            var response = new CommodityReadDTO
            {
                CommodityId = existing.CommodityId,
                HotelId = existing.HotelId,
                HotelName = hotel.Name,
                HasParking = existing.HasParking,
                IsParkingPaid = existing.IsParkingPaid,
                ParkingPrice = existing.ParkingPrice,
                HasBreakfast = existing.HasBreakfast,
                IsBreakfastPaid = existing.IsBreakfastPaid,
                BreakfastPrice = existing.BreakfastPrice,
                HasLunch = existing.HasLunch,
                IsLunchPaid = existing.IsLunchPaid,
                LunchPrice = existing.LunchPrice,
                HasDinner = existing.HasDinner,
                IsDinnerPaid = existing.IsDinnerPaid,
                DinnerPrice = existing.DinnerPrice,
                HasSpa = existing.HasSpa,
                IsSpaPaid = existing.IsSpaPaid,
                SpaPrice = existing.SpaPrice,
                HasPool = existing.HasPool,
                IsPoolPaid = existing.IsPoolPaid,
                PoolPrice = existing.PoolPrice,
                HasGym = existing.HasGym,
                IsGymPaid = existing.IsGymPaid,
                GymPrice = existing.GymPrice,
                HasWiFi = existing.HasWiFi,
                IsWiFiPaid = existing.IsWiFiPaid,
                WiFiPrice = existing.WiFiPrice,
                HasAirConditioning = existing.HasAirConditioning,
                IsAirConditioningPaid = existing.IsAirConditioningPaid,
                AirConditioningPrice = existing.AirConditioningPrice,
                HasAccessibilityFeatures = existing.HasAccessibilityFeatures,
                IsAccessibilityFeaturesPaid = existing.IsAccessibilityFeaturesPaid,
                AccessibilityFeaturesPrice = existing.AccessibilityFeaturesPrice,
                IsPetFriendly = existing.IsPetFriendly,
                IsPetFriendlyPaid = existing.IsPetFriendlyPaid,
                PetFriendlyPrice = existing.PetFriendlyPrice,
                IsActive = existing.IsActive,
                CustomCommodities = existing.CustomCommodities.Select(cs => new CustomCommodityDTO
                {
                    CustomCommodityId = cs.CustomCommodityId,
                    Name = cs.Name,
                    Description = cs.Description,
                    IsPaid = cs.IsPaid,
                    Price = cs.Price,
                    IsActive = cs.IsActive,
                    HotelName = hotel.Name,
                    HotelId = cs.HotelId,
                    CommodityId = cs.CommodityId
                }).ToList()
            };

            return Ok(new ApiResponse<CommodityReadDTO>(true, "Commodity atualizada com sucesso.", response));
        }

        // DELETE: api/Commodity/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _commodityRepository.SoftDeleteAsync(id);
            if (!deleted)
                return NotFound(new ApiResponse<Commodity>(false, "Commodity não encontrada ou já removida."));

            return NoContent();
        }
    }
}