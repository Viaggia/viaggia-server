using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Commoditie;
using viaggia_server.DTOs.Commodity;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.Hotels;
using viaggia_server.Repositories;
using viaggia_server.Repositories.Commodities;
using viaggia_server.Repositories.HotelRepository;
using viaggia_server.Services;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommoditieController : ControllerBase
    {
        private readonly ICommoditieRepository _commoditieRepository;
        private readonly IHotelRepository _hotelRepository;
       

        public CommoditieController(
            ICommoditieRepository commoditieRepository,
            IHotelRepository hotelRepository)
        {
            _commoditieRepository = commoditieRepository;
            _hotelRepository = hotelRepository;
            
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var commodities = await _commoditieRepository.GetAllAsync(); // já inclui Hotel com Include

            var dtoList = commodities.Select(c => new CommoditieReadDTO
            {
                CommoditieId = c.CommoditieId,
                HotelName = c.Hotel.Name,
                HotelId = c.HotelId,
                HasParking = c.HasParking,
                IsParkingPaid = c.IsParkingPaid,
                HasBreakfast = c.HasBreakfast,
                IsBreakfastPaid = c.IsBreakfastPaid,
                HasLunch = c.HasLunch,
                IsLunchPaid = c.IsLunchPaid,
                HasDinner = c.HasDinner,
                IsDinnerPaid = c.IsDinnerPaid,
                HasSpa = c.HasSpa,
                IsSpaPaid = c.IsSpaPaid,
                HasPool = c.HasPool,
                IsPoolPaid = c.IsPoolPaid,
                HasGym = c.HasGym,
                IsGymPaid = c.IsGymPaid,
                HasWiFi = c.HasWiFi,
                IsWiFiPaid = c.IsWiFiPaid,
                HasAirConditioning = c.HasAirConditioning,
                IsAirConditioningPaid = c.IsAirConditioningPaid,
                HasAccessibilityFeatures = c.HasAccessibilityFeatures,
                IsAccessibilityFeaturesPaid = c.IsAccessibilityFeaturesPaid,
                IsPetFriendly = c.IsPetFriendly,
                IsPetFriendlyPaid = c.IsPetFriendlyPaid,
                IsActive = c.IsActive
           
            });

            return Ok(new ApiResponse<IEnumerable<CommoditieReadDTO>>(true, "Lista de comodidades", dtoList));
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var commoditie = await _commoditieRepository.GetByIdAsync(id);
            if (commoditie == null)
                return NotFound(new ApiResponse<CommoditieDTO>(false, "Comodidade não encontrada."));

            var dto = new CommoditieDTO
            {
                CommoditieId = commoditie.CommoditieId,
                HotelId = commoditie.HotelId,
                HotelName = commoditie.Hotel?.Name,
                HasParking = commoditie.HasParking,
                IsParkingPaid = commoditie.IsParkingPaid,
                HasBreakfast = commoditie.HasBreakfast,
                IsBreakfastPaid = commoditie.IsBreakfastPaid,
                HasLunch = commoditie.HasLunch,
                IsLunchPaid = commoditie.IsLunchPaid,
                HasDinner = commoditie.HasDinner,
                IsDinnerPaid = commoditie.IsDinnerPaid,
                HasSpa = commoditie.HasSpa,
                IsSpaPaid = commoditie.IsSpaPaid,
                HasPool = commoditie.HasPool,
                IsPoolPaid = commoditie.IsPoolPaid,
                HasGym = commoditie.HasGym,
                IsGymPaid = commoditie.IsGymPaid,
                HasWiFi = commoditie.HasWiFi,
                IsWiFiPaid = commoditie.IsWiFiPaid,
                HasAirConditioning = commoditie.HasAirConditioning,
                IsAirConditioningPaid = commoditie.IsAirConditioningPaid,
                HasAccessibilityFeatures = commoditie.HasAccessibilityFeatures,
                IsAccessibilityFeaturesPaid = commoditie.IsAccessibilityFeaturesPaid,
                IsPetFriendly = commoditie.IsPetFriendly,
                IsPetFriendlyPaid = commoditie.IsPetFriendlyPaid,
                IsActive = commoditie.IsActive
                // CommoditieServices não será incluído
            };

            return Ok(new ApiResponse<CommoditieDTO>(true, "Comodidade encontrada.", dto));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateCommoditieDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<CreateCommoditieDTO>(false, "Dados inválidos.", null, ModelState));

            var hotel = await _hotelRepository.GetHotelByNameAsync(dto.HotelName?.Trim());
            if (hotel == null)
                return BadRequest(new ApiResponse<object>(false, $"Hotel com nome '{dto.HotelName}' não encontrado."));

            var commoditie = new Commoditie
            {
                HotelId = hotel.HotelId,
                HasParking = dto.HasParking,
                IsParkingPaid = dto.IsParkingPaid,
                HasBreakfast = dto.HasBreakfast,
                IsBreakfastPaid = dto.IsBreakfastPaid,
                HasLunch = dto.HasLunch,
                IsLunchPaid = dto.IsLunchPaid,
                HasDinner = dto.HasDinner,
                IsDinnerPaid = dto.IsDinnerPaid,
                HasSpa = dto.HasSpa,
                IsSpaPaid = dto.IsSpaPaid,
                HasPool = dto.HasPool,
                IsPoolPaid = dto.IsPoolPaid,
                HasGym = dto.HasGym,
                IsGymPaid = dto.IsGymPaid,
                HasWiFi = dto.HasWiFi,
                IsWiFiPaid = dto.IsWiFiPaid,
                HasAirConditioning = dto.HasAirConditioning,
                IsAirConditioningPaid = dto.IsAirConditioningPaid,
                HasAccessibilityFeatures = dto.HasAccessibilityFeatures,
                IsAccessibilityFeaturesPaid = dto.IsAccessibilityFeaturesPaid,
                IsPetFriendly = dto.IsPetFriendly,
                IsPetFriendlyPaid = dto.IsPetFriendlyPaid,
                IsActive = dto.IsActive
            };

            var result = await _commoditieRepository.AddAsync(commoditie);

            var response = new
            {
                Commoditie = result,
                HotelId = hotel.HotelId,
                HotelName = hotel.Name
            };

            return Ok(new ApiResponse<object>(true, "Comodidade criada com sucesso.", response));
        }




        // PUT: api/commoditie/{id}
        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateCommoditieDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<UpdateCommoditieDTO>(false, "Dados inválidos.", null, ModelState));

            var existing = await _commoditieRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new ApiResponse<Commoditie>(false, "Comodidade não encontrada."));

            var hotel = await _hotelRepository.GetHotelByNameAsync(dto.HotelName);
            if (hotel == null)
                return BadRequest(new ApiResponse<object>(false, $"Hotel com nome '{dto.HotelName}' não encontrado."));

            
            existing.HotelId = hotel.HotelId;
            existing.HasParking = dto.HasParking;
            existing.IsParkingPaid = dto.IsParkingPaid;
            existing.HasBreakfast = dto.HasBreakfast;
            existing.IsBreakfastPaid = dto.IsBreakfastPaid;
            existing.HasLunch = dto.HasLunch;
            existing.IsLunchPaid = dto.IsLunchPaid;
            existing.HasDinner = dto.HasDinner;
            existing.IsDinnerPaid = dto.IsDinnerPaid;
            existing.HasSpa = dto.HasSpa;
            existing.IsSpaPaid = dto.IsSpaPaid;
            existing.HasPool = dto.HasPool;
            existing.IsPoolPaid = dto.IsPoolPaid;
            existing.HasGym = dto.HasGym;
            existing.IsGymPaid = dto.IsGymPaid;
            existing.HasWiFi = dto.HasWiFi;
            existing.IsWiFiPaid = dto.IsWiFiPaid;
            existing.HasAirConditioning = dto.HasAirConditioning;
            existing.IsAirConditioningPaid = dto.IsAirConditioningPaid;
            existing.HasAccessibilityFeatures = dto.HasAccessibilityFeatures;
            existing.IsAccessibilityFeaturesPaid = dto.IsAccessibilityFeaturesPaid;
            existing.IsPetFriendly = dto.IsPetFriendly;
            existing.IsPetFriendlyPaid = dto.IsPetFriendlyPaid;
            existing.IsActive = dto.IsActive;

            await _commoditieRepository.UpdateAsync(existing);

            return Ok(new ApiResponse<Commoditie>(true, "Comodidade atualizada com sucesso.", existing));
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _commoditieRepository.SoftDeleteAsync(id);
            if (!deleted)
                return NotFound(new ApiResponse<Commoditie>(false, "Comodidade não encontrada ou já removida."));

            return Ok(new ApiResponse<object>(true, "Comodidade removida com sucesso."));
        }

    }
}
