using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.CustomCommodities;

namespace viaggia_server.Repositories.CommodityRepository
{
    public class CustomCommodityRepository : ICustomCommodityRepository
    {
        private readonly AppDbContext _context;

        public CustomCommodityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomCommodity>> GetAllAsync()
        {
            return await _context.CustomCommodities
                .Where(s => s.IsActive)
                .OrderBy(s => s.CustomCommodityId)
                .ToListAsync();
        }

        public async Task<CustomCommodity?> GetByIdAsync(int id)
        {
            return await _context.CustomCommodities
                 .Include(cs => cs.Hotel)
                .FirstOrDefaultAsync(s => s.CustomCommodityId == id && s.IsActive);
        }

        public async Task<IEnumerable<CustomCommodity>> GetByCommodityIdAsync(int commoditieId)
        {
            return await _context.CustomCommodities
                .Where(s => s.CommodityId == commoditieId && s.IsActive)
                .Include(s => s.Hotel)
                .OrderBy(s => s.CustomCommodityId)
                .ToListAsync();
        }

        public async Task<CustomCommodity> AddAsync(CustomCommodity entity)
        {
            await _context.CustomCommodities.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<CustomCommodity> UpdateAsync(CustomCommodity entity)
        {
            _context.CustomCommodities.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                return false;

            entity.IsActive = false;
            _context.CustomCommodities.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }



        public async Task<bool> SaveChangesAsync()
        {
            var changes = await _context.SaveChangesAsync();
            return changes > 0;
        }

        Task<T2?> IRepository<CustomCommodity>.GetByIdAsync<T2>(int id) where T2 : class
        {
            throw new NotImplementedException();
        }

        Task<bool> IRepository<CustomCommodity>.SoftDeleteAsync<T2>(int id)
        {
            throw new NotImplementedException();
        }

        public Task<CustomCommodity?> GetByIdWithIncludesAsync(int id, params Expression<Func<CustomCommodity, object>>[] includes)
        {
            throw new NotImplementedException();
        }
    }
}

