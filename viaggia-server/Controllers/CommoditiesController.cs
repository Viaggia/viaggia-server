using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Commoditie;
using viaggia_server.Models.Commodities;
using viaggia_server.Repositories.Commodities;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommoditieController : ControllerBase
    {
        private readonly ICommoditieRepository _commoditieRepository;

        public CommoditieController(ICommoditieRepository commoditieRepository)
        {
            _commoditieRepository = commoditieRepository;
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

        // POST: api/commoditie
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CommoditieDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<CommoditieDTO>(false, "Dados inválidos.", null, ModelState));

            var commoditie = new Commoditie
            {
                HotelId = dto.HotelId,
                HasParking = dto.HasParking,
                HasBreakfast = dto.HasBreakfast,
                HasLunch = dto.HasLunch,
                HasDinner = dto.HasDinner,
                HasSpa = dto.HasSpa,
                HasPool = dto.HasPool,
                HasGym = dto.HasGym,
                IsActive = true
            };

            var created = await _commoditieRepository.AddAsync(commoditie);
            return CreatedAtAction(nameof(GetById), new { id = created.CommoditieId },
                new ApiResponse<Commoditie>(true, "Commodity criada com sucesso.", created));
        }

        // PUT: api/commoditie/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CommoditieDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<CommoditieDTO>(false, "Dados inválidos.", null, ModelState));

            var existing = await _commoditieRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new ApiResponse<Commoditie>(false, "Commodity não encontrada."));

            existing.HasParking = dto.HasParking;
            existing.HasBreakfast = dto.HasBreakfast;
            existing.HasLunch = dto.HasLunch;
            existing.HasDinner = dto.HasDinner;
            existing.HasSpa = dto.HasSpa;
            existing.HasPool = dto.HasPool;
            existing.HasGym = dto.HasGym;
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