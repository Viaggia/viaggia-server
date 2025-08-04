using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.CustomCommodities;

namespace viaggia_server.Repositories.CommodityRepository
{
    public class CustomCommodityRepository : ICustomCommodityRepository
    {
        private readonly AppDbContext _context;

        public CustomCommodityRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<CustomCommodity>> GetAllAsync()
        {
            return await _context.CustomCommodities
                .Include(cs => cs.Hotel)
                .Include(cs => cs.Commodity)
                .Where(s => s.IsActive)
                .OrderBy(s => s.CustomCommodityId)
                .ToListAsync();
        }

        public async Task<CustomCommodity?> GetByIdAsync(int id)
        {
            return await _context.CustomCommodities
                .Include(cs => cs.Hotel)
                .Include(cs => cs.Commodity)
                .FirstOrDefaultAsync(s => s.CustomCommodityId == id && s.IsActive);
        }

        public async Task<IEnumerable<CustomCommodity>> GetByCommodityIdAsync(int commodityId)
        {
            return await _context.CustomCommodities
                .Include(cs => cs.Hotel)
                .Include(cs => cs.Commodity)
                .Where(s => s.CommodityId == commodityId && s.IsActive)
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
            return await _context.SaveChangesAsync() > 0;
        }

        Task<T2?> IRepository<CustomCommodity>.GetByIdAsync<T2>(int id) where T2 : class
        {
            throw new NotImplementedException();
        }

        Task<bool> IRepository<CustomCommodity>.SoftDeleteAsync<T2>(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<CustomCommodity?> GetByIdWithIncludesAsync(int id, params Expression<Func<CustomCommodity, object>>[] includes)
        {
            var query = _context.CustomCommodities.AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.FirstOrDefaultAsync(s => s.CustomCommodityId == id && s.IsActive);
        }
    }
}