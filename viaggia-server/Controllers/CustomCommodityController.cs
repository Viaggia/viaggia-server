using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Commodity;
using viaggia_server.Models.CustomCommodities;
using viaggia_server.Repositories.CommodityRepository;
using viaggia_server.Repositories.HotelRepository;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomCommodityController : ControllerBase
    {
        private readonly ICustomCommodityRepository _customCommodityRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly ICommodityRepository _commodityRepository;

        public CustomCommodityController(
            ICustomCommodityRepository customCommodityRepository,
            IHotelRepository hotelRepository,
            ICommodityRepository commodityRepository)
        {
            _customCommodityRepository = customCommodityRepository;
            _hotelRepository = hotelRepository;
            _commodityRepository = commodityRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customCommodities = await _customCommodityRepository.GetAllAsync();
            var response = customCommodities.Select(s => new CustomCommodityResponseDTO
            {
                CustomCommodityId = s.CustomCommodityId,
                Name = s.Name,
                Description = s.Description,
                IsPaid = s.IsPaid,
                Price = s.Price,
                IsActive = s.IsActive,
                HotelName = s.Hotel?.Name ?? string.Empty
            });
            return Ok(new ApiResponse<IEnumerable<CustomCommodityResponseDTO>>(true, "Comodidades encontradas.", response));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customCommodity = await _customCommodityRepository.GetByIdAsync(id);
            if (customCommodity == null)
                return NotFound(new ApiResponse<CustomCommodityDTO>(false, "Comodidade não encontrada."));

            var response = new CustomCommodityDTO
            {
                CustomCommodityId = customCommodity.CustomCommodityId,
                Name = customCommodity.Name,
                Description = customCommodity.Description,
                IsPaid = customCommodity.IsPaid,
                Price = customCommodity.Price,
                IsActive = customCommodity.IsActive,
                HotelName = customCommodity.Hotel?.Name ?? string.Empty,
                HotelId = customCommodity.HotelId,
                CommoditieId = customCommodity.CommodityId
            };
            return Ok(new ApiResponse<CustomCommodityDTO>(true, "Comodidade encontrada.", response));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateCustomCommodityDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<CreateCustomCommodityDTO>(false, "Dados inválidos.", null, ModelState));

            var hotel = await _hotelRepository.GetHotelByNameAsync(dto.HotelName);
            if (hotel == null)
                return BadRequest(new ApiResponse<object>(false, $"Hotel com nome '{dto.HotelName}' não encontrado."));

            var commodity = await _commodityRepository.GetByHotelIdAsync(hotel.HotelId);
            if (commodity == null)
                return BadRequest(new ApiResponse<object>(false, $"Comodidade associada ao hotel '{hotel.Name}' não encontrada."));

            var customCommodity = new CustomCommodity
            {
                Name = dto.Name,
                Description = dto.Description,
                IsPaid = dto.IsPaid,
                Price = dto.Price,
                IsActive = true,
                HotelId = hotel.HotelId,
                CommodityId = commodity.CommodityId
            };

            var created = await _customCommodityRepository.AddAsync(customCommodity);

            var response = new CustomCommodityDTO
            {
                CustomCommodityId = created.CustomCommodityId,
                Name = created.Name,
                Description = created.Description,
                IsPaid = created.IsPaid,
                Price = created.Price,
                IsActive = created.IsActive,
                HotelName = hotel.Name,
                HotelId = created.HotelId,
                CommoditieId = created.CommodityId
            };

            return CreatedAtAction(nameof(GetById), new { id = created.CustomCommodityId },
                new ApiResponse<CustomCommodityDTO>(true, "Comodidade criada com sucesso.", response));
        }

        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateCustomCommodityDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<UpdateCustomCommodityDTO>(false, "Dados inválidos.", null, ModelState));

            var existing = await _customCommodityRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new ApiResponse<CustomCommodity>(false, "Comodidade não encontrada."));

            var hotel = await _hotelRepository.GetHotelByNameAsync(dto.HotelName);
            if (hotel == null)
                return BadRequest(new ApiResponse<object>(false, $"Hotel com nome '{dto.HotelName}' não encontrado."));

            var commodity = await _commodityRepository.GetByHotelIdAsync(hotel.HotelId);
            if (commodity == null)
                return BadRequest(new ApiResponse<object>(false, $"Comodidade associada ao hotel '{hotel.Name}' não encontrada."));

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.IsPaid = dto.IsPaid;
            existing.Price = dto.Price;
            existing.IsActive = dto.IsActive;
            existing.HotelId = hotel.HotelId;
            existing.CommodityId = commodity.CommodityId;

            var updated = await _customCommodityRepository.UpdateAsync(existing);

            var response = new CustomCommodityDTO
            {
                CustomCommodityId = updated.CustomCommodityId,
                Name = updated.Name,
                Description = updated.Description,
                IsPaid = updated.IsPaid,
                Price = updated.Price,
                IsActive = updated.IsActive,
                HotelName = hotel.Name,
                HotelId = updated.HotelId,
                CommoditieId = updated.CommodityId
            };

            return Ok(new ApiResponse<CustomCommodityDTO>(true, "Comodidade atualizada com sucesso.", response));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _customCommodityRepository.SoftDeleteAsync(id);
            if (!deleted)
                return NotFound(new ApiResponse<CustomCommodity>(false, "Comodidade não encontrada ou já desativada."));

            return Ok(new ApiResponse<string>(true, "Comodidade desativada com sucesso."));
        }
    }
}