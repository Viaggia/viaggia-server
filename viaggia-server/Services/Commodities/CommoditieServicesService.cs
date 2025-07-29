using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using viaggia_server.Data;
using viaggia_server.DTOs.Commoditie;
using viaggia_server.DTOs.Commodity;
using viaggia_server.Models.Commodities;

namespace viaggia_server.Services
{
    public class CommoditieServicesService
    {
        private readonly AppDbContext _context;

        public CommoditieServicesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CommoditieServicesDTO>> GetByCommoditieIdAsync(int commoditieId)
        {
            var services = await _context.CommoditieServices
                .Where(s => s.CommoditieId == commoditieId && s.IsActive)
                .ToListAsync();

            return services.Select(s => new CommoditieServicesDTO
            {
                Name = s.Name,
                IsPaid = !s.IsPaid,
                Description = s.Description,
                IsActive = s.IsActive
            }).ToList();
        }

        public async Task<CommoditieServices> AddServiceAsync(int commoditieId, CommoditieServicesDTO dto)
        {
            var service = new CommoditieServices
            {
                CommoditieId = commoditieId,
                Name = dto.Name,
                IsPaid = !dto.IsPaid,
                Description = dto.Description,
                IsActive = dto.IsActive
            };

            _context.CommoditieServices.Add(service);
            await _context.SaveChangesAsync();

            return service;
        }

        public async Task<bool> UpdateServiceAsync(int serviceId, CommoditieServicesDTO dto)
        {
            var service = await _context.CommoditieServices.FindAsync(serviceId);
            if (service == null) return false;

            service.Name = dto.Name;
            service.IsPaid = !dto.IsPaid;
            service.Description = dto.Description;
            service.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteServiceAsync(int serviceId)
        {
            var service = await _context.CommoditieServices.FindAsync(serviceId);
            if (service == null) return false;

            // Soft delete, se implementar ISoftDeletable
            service.IsActive = false;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
