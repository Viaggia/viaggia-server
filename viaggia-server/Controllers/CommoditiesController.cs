using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Commoditie;
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
        private readonly IRepository<Hotel> _genericRepository;

        public CommoditieController(
            ICommoditieRepository commoditieRepository,
            IHotelRepository hotelRepository,
            IRepository<Hotel> genericRepository)
        {
            _commoditieRepository = commoditieRepository;
            _hotelRepository = hotelRepository;
            _genericRepository = genericRepository;
        }

        // GET: api/commoditie
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var commodities = await _commoditieRepository.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<Commoditie>>(true, "Commodities recuperadas com sucesso.", commodities));
        }

        // GET: api/commoditie/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var commoditie = await _commoditieRepository.GetByIdAsync(id);
            if (commoditie == null)
                return NotFound(new ApiResponse<Commoditie>(false, "Commodity não encontrada."));

            return Ok(new ApiResponse<Commoditie>(true, "Commodity encontrada.", commoditie));
        }

        // GET: api/commoditie/hotel/{hotelId}
        [HttpGet("hotel/{hotelId:int}")]
        public async Task<IActionResult> GetByHotelId(int hotelId)
        {
            var commoditie = await _commoditieRepository.GetByHotelIdAsync(hotelId);
            if (commoditie == null)
                return NotFound(new ApiResponse<Commoditie>(false, "Commodity não encontrada para este hotel."));

            return Ok(new ApiResponse<Commoditie>(true, "Commodity do hotel encontrada.", commoditie));
        }

        // GET: api/commoditie/list-hotels (lista hotéis id + nome)
        [HttpGet("list-hotels")]
        public async Task<IActionResult> GetHotelsList()
        {
            var hotels = await _genericRepository.GetAllAsync();
            var list = hotels.Select(h => new { h.HotelId, h.Name }).ToList();
            return Ok(new ApiResponse<object>(true, "Lista de hotéis", list));
        }

        // POST: api/commoditie
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CommoditieCreateByHotelNameDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<CommoditieCreateByHotelNameDTO>(false, "Dados inválidos.", null, ModelState));

            var hotel = await _hotelRepository.GetHotelByNameAsync(dto.HotelName);
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

            // Retornar dados incluindo o nome do hotel junto com a commoditie criada
            var response = new
            {
                Commoditie = result,
                HotelName = hotel.Name
            };

            return Ok(new ApiResponse<object>(true, "Commoditie criada com sucesso.", response));
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
                return NotFound(new ApiResponse<Commoditie>(false, "Commodity não encontrada."));

            var hotel = await _hotelRepository.GetHotelByNameAsync(dto.HotelName);
            if (hotel == null)
                return BadRequest(new ApiResponse<object>(false, $"Hotel com nome '{dto.HotelName}' não encontrado."));

            // Atualiza os dados da commodity
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

            return Ok(new ApiResponse<Commoditie>(true, "Commodity atualizada com sucesso.", existing));
        }

        // DELETE: api/commoditie/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _commoditieRepository.SoftDeleteAsync(id);
            if (!deleted)
                return NotFound(new ApiResponse<Commoditie>(false, "Commodity não encontrada ou já removida."));

            return NoContent();
        }
    }
}
