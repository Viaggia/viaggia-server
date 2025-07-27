using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Commodities;

namespace viaggia_server.Repositories.Commodities
{
    public class CommoditieServicesRepository : ICommoditieServicesRepository
    {
        private readonly AppDbContext _context;

        public CommoditieServicesRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CommoditieServices>> GetAllAsync()
        {
            return await _context.CommoditiesServices
                .Where(s => s.IsActive)
                .ToListAsync();
        }

        public async Task<CommoditieServices?> GetByIdAsync(int id)
        {
            return await _context.CommoditiesServices
                .FirstOrDefaultAsync(s => s.CommoditieServicesId == id && s.IsActive);
        }

        public async Task<IEnumerable<CommoditieServices>> GetByCommoditieIdAsync(int commoditieId)
        {
            return await _context.CommoditiesServices
                .Where(s => s.CommoditieId == commoditieId && s.IsActive)
                .ToListAsync();
        }

        public async Task<CommoditieServices> AddAsync(CommoditieServices entity)
        {
            await _context.CommoditiesServices.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<CommoditieServices> UpdateAsync(CommoditieServices entity)
        {
            _context.CommoditiesServices.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                return false;

            entity.IsActive = false;
            _context.CommoditiesServices.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
    

