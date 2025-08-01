using Microsoft.AspNetCore.Mvc;
using viaggia_server.DTOs;
using viaggia_server.DTOs.Commoditie;
using viaggia_server.Models.Commodities;
using viaggia_server.Repositories.Commodities;

namespace viaggia_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommoditieServicesController : ControllerBase
    {
        private readonly ICommoditieServicesRepository _serviceRepository;

        public CommoditieServicesController(ICommoditieServicesRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        // GET: api/commoditieservices
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var services = await _serviceRepository.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<CommoditieServices>>(true, "Serviços personalizados encontrados.", services));
        }

        // GET: api/commoditieservices/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);
            if (service == null)
                return NotFound(new ApiResponse<CommoditieServices>(false, "Serviço personalizado não encontrado."));

            return Ok(new ApiResponse<CommoditieServices>(true, "Serviço personalizado encontrado.", service));
        }

        // GET: api/commoditieservices/commoditie/{commoditieId}
        [HttpGet("commoditie/{commoditieId:int}")]
        public async Task<IActionResult> GetByCommoditieId(int commoditieId)
        {
            var services = await _serviceRepository.GetByCommoditieIdAsync(commoditieId);
            if (!services.Any())
                return NotFound(new ApiResponse<IEnumerable<CommoditieServices>>(false, "Nenhum serviço encontrado para esta commodity."));

            return Ok(new ApiResponse<IEnumerable<CommoditieServices>>(true, "Serviços encontrados para a commodity.", services));
        }

        // POST: api/commoditieservices
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CommoditieServicesDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<CommoditieServicesDTO>(false, "Dados inválidos.", null, ModelState));

            var service = new CommoditieServices
            {
                CommoditieId = dto.CommoditieId,
                Name = dto.Name,
                IsPaid = dto.IsPaid,
                IsActive = true
            };

            var created = await _serviceRepository.AddAsync(service);

            return CreatedAtAction(nameof(GetById), new { id = created.CommoditieServicesId },
                new ApiResponse<CommoditieServices>(true, "Serviço personalizado criado com sucesso.", created));
        }

        // PUT: api/commoditieservices/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CommoditieServicesDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<CommoditieServicesDTO>(false, "Dados inválidos.", null, ModelState));

            var existing = await _serviceRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new ApiResponse<CommoditieServices>(false, "Serviço não encontrado."));

            existing.Name = dto.Name;
            existing.IsPaid = dto.IsPaid;
            existing.IsActive = dto.IsActive;

            await _serviceRepository.UpdateAsync(existing);
            return Ok(new ApiResponse<CommoditieServices>(true, "Serviço personalizado atualizado com sucesso.", existing));
        }

        // DELETE: api/commoditieservices/{id}
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