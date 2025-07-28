using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using viaggia_server.Data;
using viaggia_server.DTOs.Commodities;
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
            var services = await _context.CommoditiesServices
                .Where(s => s.CommoditieId == commoditieId && s.IsActive)
                .ToListAsync();

            return services.Select(s => new CommoditieServicesDTO
            {
                ServiceName = s.Name,
                IsFree = !s.IsPaid,
                IsActive = s.IsActive
            }).ToList();
        }

        public async Task<CommoditieServices> AddServiceAsync(int commoditieId, CommoditieServicesDTO dto)
        {
            var service = new CommoditieServices
            {
                CommoditieId = commoditieId,
                Name = dto.ServiceName,
                IsPaid = !dto.IsFree,
                IsActive = dto.IsActive
            };

            _context.CommoditiesServices.Add(service);
            await _context.SaveChangesAsync();

            return service;
        }

        public async Task<bool> UpdateServiceAsync(int serviceId, CommoditieServicesDTO dto)
        {
            var service = await _context.CommoditiesServices.FindAsync(serviceId);
            if (service == null) return false;

            service.Name = dto.ServiceName;
            service.IsPaid = !dto.IsFree;
            service.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteServiceAsync(int serviceId)
        {
            var service = await _context.CommoditiesServices.FindAsync(serviceId);
            if (service == null) return false;

            // Soft delete, se implementar ISoftDeletable
            service.IsActive = false;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
