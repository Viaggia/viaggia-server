using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.Hotels;
using viaggia_server.Repositories;
using viaggia_server.Repositories.Commodities;

namespace viaggia_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommoditiesController : ControllerBase
    {
        private readonly IRepository<Commoditie> _genericRepository;
        private readonly ICommoditieRepository _commoditieRepository;

        public CommoditiesController(ICommoditieRepository commoditieRepository)
        {
            _commoditieRepository = commoditieRepository;
        }

        // GET: api/commodities/hotel/{hotelId}
        [HttpGet("hotel/{hotelId:int}")]
        public async Task<IActionResult> GetCommoditieByHotelId(int hotelId)
        {
            var commoditie = await _commoditieRepository.GetByHotelIdAsync(hotelId);
            if (commoditie == null)
                return NotFound($"Nenhuma commoditie encontrada para o hotel {hotelId}.");
            return Ok(commoditie);
        }

        // POST: api/commodities/hotel/{hotelId}
        [HttpPost("hotel/{hotelId:int}")]
        public async Task<IActionResult> AddCommoditieToHotel(int hotelId, [FromBody] Commoditie commoditie)
        {
            if (commoditie == null)
                return BadRequest("Dados da commoditie são obrigatórios.");

            commoditie.HotelId = hotelId;
            var result = await _genericRepository.AddAsync(commoditie);
            return CreatedAtAction(nameof(GetCommoditieByHotelId), new { hotelId = hotelId }, result);
        }

        // PUT: api/commodities/hotel/{hotelId}
        [HttpPut("hotel/{hotelId:int}")]
        public async Task<IActionResult> UpdateCommoditieByHotelId(int hotelId, [FromBody] Commoditie commoditie)
        {
            var existing = await _commoditieRepository.GetByHotelIdAsync(hotelId);
            if (existing == null)
                return NotFound($"Nenhuma commoditie encontrada para o hotel {hotelId}.");

            commoditie.CommoditieId = existing.CommoditieId;
            commoditie.HotelId = hotelId;
            var updated = await _genericRepository.UpdateAsync(commoditie);
            return Ok(updated);
        }

        // DELETE (soft): api/commodities/hotel/{hotelId}
        [HttpDelete("hotel/{hotelId:int}")]
        public async Task<IActionResult> SoftDeleteCommoditieByHotelId(int hotelId)
        {
            var commoditie = await _commoditieRepository.GetByHotelIdAsync(hotelId);
            if (commoditie == null)
                return NotFound($"Nenhuma commoditie encontrada para o hotel {hotelId}.");

            commoditie.IsActive = false;
            await _genericRepository.UpdateAsync(commoditie);
            return NoContent();
        }
    }
}
