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
        private readonly ICustomCommodityRepository _serviceRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly ICommodityRepository _commoditieRepository;


        public CustomCommodityController(
            ICustomCommodityRepository serviceRepository,
            IHotelRepository hotelRepository,
            ICommodityRepository commoditieRepository)
        {
            _serviceRepository = serviceRepository;
            _hotelRepository = hotelRepository;
            _commoditieRepository = commoditieRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var services = await _serviceRepository.GetAllAsync();

            var response = services.Select(s => new CustomCommodityResponseDTO
            {
                CustomCommodityId = s.CustomCommodityId,
                Name = s.Name,
                Description = s.Description,
                IsPaid = s.IsPaid,
                IsActive = s.IsActive,
                HotelName = s.Hotel?.Name ?? string.Empty
            });

            return Ok(new ApiResponse<IEnumerable<CustomCommodityResponseDTO>>(true, "Comodidades encontradas.", response));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);
            if (service == null)
                return NotFound(new ApiResponse<CustomCommodityDTO>(false, "Comodidade não encontrada."));


            var response = new CustomCommodityDTO
            {
                CustomCommodityId = service.CustomCommodityId,
                Name = service.Name,
                Description = service.Description,
                IsPaid = service.IsPaid,
                IsActive = service.IsActive,
                HotelName = service.Hotel?.Name ?? string.Empty,
                HotelId = service.HotelId,
                CommodityId = service.CommodityId
            };


            return Ok(new ApiResponse<CustomCommodityDTO>(true, "Comodidade encontrada.", response));
        }


        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateCustomCommodityDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<CreateCustomCommodityDTO>(false, "Dados inválidos.", null, ModelState));

            // Buscar hotel pelo nome
            var hotel = await _hotelRepository.GetHotelByNameAsync(dto.HotelName);
            if (hotel == null)
                return BadRequest(new ApiResponse<object>(false, $"Hotel com nome '{dto.HotelName}' não encontrado."));

            // Buscar commoditie associada ao hotel
            var commoditie = (await _commoditieRepository.GetAllAsync())
                .FirstOrDefault(c => c.HotelId == hotel.HotelId);

            if (commoditie == null)
                return BadRequest(new ApiResponse<object>(false, $"Comodidade associada ao hotel '{hotel.Name}' não encontrada."));

            var service = new CustomCommodity
            {
                Name = dto.Name,
                Description = dto.Description,
                IsPaid = dto.IsPaid,
                IsActive = true,
                HotelId = hotel.HotelId,
                CommodityId = commoditie.CommodityId
            };

            var created = await _serviceRepository.AddAsync(service);

            return CreatedAtAction(nameof(GetById), new { id = created.CustomCommodityId },
                new ApiResponse<CustomCommodity>(true, "Comodidade criada com sucesso.", created));
        }

        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateCustomCommodityDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<UpdateCustomCommodityDTO>(false, "Dados inválidos.", null, ModelState));

            var existing = await _serviceRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new ApiResponse<CustomCommodity>(false, "Comodidade não encontrada."));

            // Atualiza apenas os campos permitidos
            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.IsPaid = dto.IsPaid;
            existing.IsActive = dto.IsActive;

            var updated = await _serviceRepository.UpdateAsync(existing);

            return Ok(new ApiResponse<CustomCommodity>(true, "Comodidade atualizada com sucesso.", updated));
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _serviceRepository.SoftDeleteAsync(id);
            if (!deleted)
                return NotFound(new ApiResponse<CustomCommodity>(false, "Comodidade não encontrada ou já desativada."));

            return Ok(new ApiResponse<string>(true, "Comodidade desativada com sucesso."));
        }

    }
}
