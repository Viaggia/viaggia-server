using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Commoditie;
using viaggia_server.Services.Commodities;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommoditieController : ControllerBase
    {
        private readonly ICommoditieService _commoditieService;

        public CommoditieController(ICommoditieService commoditieService)
        {
            _commoditieService = commoditieService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var commodities = await _commoditieService.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<CommoditieDTO>>(true, "Commodities retrieved successfully.", commodities));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var commoditie = await _commoditieService.GetByIdAsync(id);
            if (commoditie == null)
                return NotFound(new ApiResponse<CommoditieDTO>(false, "Commodity not found."));

            return Ok(new ApiResponse<CommoditieDTO>(true, "Commodity found.", commoditie));
        }

        [HttpGet("hotel/{hotelId:int}")]
        public async Task<IActionResult> GetByHotelId(int hotelId)
        {
            var commoditie = await _commoditieService.GetByHotelIdAsync(hotelId);
            if (commoditie == null)
                return NotFound(new ApiResponse<CommoditieDTO>(false, "Commodity not found for this hotel."));

            return Ok(new ApiResponse<CommoditieDTO>(true, "Commodity found.", commoditie));
        }

        [HttpGet("hotel/{hotelId:int}/list")]
        public async Task<IActionResult> GetByHotelIdList(int hotelId)
        {
            var commodities = await _commoditieService.GetByHotelIdListAsync(hotelId);
            return Ok(new ApiResponse<IEnumerable<CommoditieDTO>>(true, "Commodities found.", commodities));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCommoditieDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<CommoditieDTO>(false, "Invalid data.", null, ModelState));

            try
            {
                var created = await _commoditieService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.CommoditieId },
                    new ApiResponse<CommoditieDTO>(true, "Commodity created successfully.", created));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<CommoditieDTO>(false, ex.Message));
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCommoditieDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<CommoditieDTO>(false, "Invalid data.", null, ModelState));

            try
            {
                var updated = await _commoditieService.UpdateAsync(id, dto);
                return Ok(new ApiResponse<CommoditieDTO>(true, "Commodity updated successfully.", updated));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<CommoditieDTO>(false, ex.Message));
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _commoditieService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new ApiResponse<CommoditieDTO>(false, "Commodity not found or already deleted."));

            return NoContent();
        }
    }
}