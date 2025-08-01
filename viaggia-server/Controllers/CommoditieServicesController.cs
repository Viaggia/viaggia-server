using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Commoditie;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.Hotels;
using viaggia_server.Repositories;
using viaggia_server.Repositories.Commodities;
using viaggia_server.Repositories.HotelRepository;
using viaggia_server.Data;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommoditieServicesController : ControllerBase
    {
        private readonly ICommoditieServicesRepository _serviceRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IRepository<Hotel> _genericRepository;
        private readonly AppDbContext _context;

        public CommoditieServicesController(
            ICommoditieServicesRepository serviceRepository,
            IHotelRepository hotelRepository,
            IRepository<Hotel> genericRepo,
            AppDbContext context)
        {
            _serviceRepository = serviceRepository;
            _hotelRepository = hotelRepository;
            _genericRepository = genericRepo;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var services = await _serviceRepository.GetAllWithHotelAsync();

            var response = services.Select(s => new CommoditieServicesResponseDTO
            {
                CommoditieServicesId = s.CommoditieServicesId,
                Name = s.Name,
                Description = s.Description,
                IsPaid = s.IsPaid,
                IsActive = s.IsActive,
                HotelName = s.Hotel?.Name ?? string.Empty
            });

            return Ok(new ApiResponse<IEnumerable<CommoditieServicesResponseDTO>>(true, "Serviços personalizados encontrados.", response));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);
            if (service == null)
                return NotFound(new ApiResponse<CommoditieServicesResponseDTO>(false, "Serviço personalizado não encontrado."));

            var response = new CommoditieServicesResponseDTO
            {
                CommoditieServicesId = service.CommoditieServicesId,
                Name = service.Name,
                Description = service.Description,
                IsPaid = service.IsPaid,
                IsActive = service.IsActive,
                HotelName = service.Hotel?.Name ?? string.Empty
            };

            return Ok(new ApiResponse<CommoditieServicesResponseDTO>(true, "Serviço personalizado encontrado.", response));
        }

        [HttpGet("commoditie/{commoditieId:int}")]
        public async Task<IActionResult> GetByCommoditieId(int commoditieId)
        {
            var services = await _serviceRepository.GetByCommoditieIdAsync(commoditieId);
            if (!services.Any())
                return NotFound(new ApiResponse<IEnumerable<CommoditieServices>>(false, "Nenhum serviço encontrado para esta commodity."));

            return Ok(new ApiResponse<IEnumerable<CommoditieServices>>(true, "Serviços encontrados para a commodity.", services));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateCommoditieServicesDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<CreateCommoditieServicesDTO>(false, "Dados inválidos.", null, ModelState));

            // Buscar hotel pelo nome
            var hotel = await _hotelRepository.GetHotelByNameAsync(dto.HotelName);
            if (hotel == null)
                return BadRequest(new ApiResponse<object>(false, $"Hotel com nome '{dto.HotelName}' não encontrado."));

            // Buscar commoditie associada ao hotel
            var commoditie = await _context.Commodities
                .FirstOrDefaultAsync(c => c.HotelId == hotel.HotelId);

            if (commoditie == null)
                return BadRequest(new ApiResponse<object>(false, $"Commoditie associada ao hotel '{hotel.Name}' não encontrada."));

            var service = new CommoditieServices
            {
                Name = dto.Name,
                Description = dto.Description,
                IsPaid = dto.IsPaid,
                IsActive = true,
                HotelId = hotel.HotelId,
                CommoditieId = commoditie.CommoditieId
            };

            var created = await _serviceRepository.AddAsync(service);

            return CreatedAtAction(nameof(GetById), new { id = created.CommoditieServicesId },
                new ApiResponse<CommoditieServices>(true, "Serviço personalizado criado com sucesso.", created));
        }

        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateCommoditeServicesDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<UpdateCommoditeServicesDTO>(false, "Dados inválidos.", null, ModelState));

            var existing = await _serviceRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new ApiResponse<CommoditieServices>(false, "Serviço personalizado não encontrado."));

            // Atualiza apenas os campos permitidos
            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.IsPaid = dto.IsPaid;
            existing.IsActive = dto.IsActive;

            var updated = await _serviceRepository.UpdateAsync(existing);

            return Ok(new ApiResponse<CommoditieServices>(true, "Serviço personalizado atualizado com sucesso.", updated));
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _serviceRepository.SoftDeleteAsync(id);
            if (!deleted)
                return NotFound(new ApiResponse<CommoditieServices>(false, "Serviço não encontrado ou já desativado."));

            return NoContent();
        }
    }
}
