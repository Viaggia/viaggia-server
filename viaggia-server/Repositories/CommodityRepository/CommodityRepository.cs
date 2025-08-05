using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Commodities;

namespace viaggia_server.Repositories.CommodityRepository
{
    public class CommodityRepository : ICommodityRepository
    {
        private readonly AppDbContext _context;

        public CommodityRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<Commodity>> GetAllAsync()
        {
            return await _context.Commodities
                .Include(c => c.CustomCommodities)
                .Include(c => c.Hotel)
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task<Commodity?> GetByIdAsync(int id)
        {
            return await _context.Commodities
                .Include(c => c.CustomCommodities)
                .Include(c => c.Hotel)
                .FirstOrDefaultAsync(c => c.CommodityId == id && c.IsActive);
        }
        public async Task<Commodity?> GetByHotelIdAsync(int hotelId)
        {
            return await _context.Commodities
                .Include(c => c.CustomCommodities)
                .Include(c => c.Hotel)
                .FirstOrDefaultAsync(c => c.HotelId == hotelId && c.IsActive);
        }

        public async Task<Commodity> AddAsync(Commodity entity)
        {
            await _context.Commodities.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Commodity> UpdateAsync(Commodity entity)
        {
            _context.Commodities.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                return false;

            entity.IsActive = false;
            _context.Commodities.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Commodities.FindAsync(id);
            if (entity == null)
                return false;

            _context.Commodities.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        Task<T2?> IRepository<Commodity>.GetByIdAsync<T2>(int id) where T2 : class
        {
            return _context.Set<T2>().FindAsync(id).AsTask();
        }

        Task<bool> IRepository<Commodity>.SoftDeleteAsync<T2>(int id)
        {
            var entity = _context.Set<T2>().Find(id);
            if (entity == null)
                return Task.FromResult(false);
            if (entity is ISoftDeletable softDeletableEntity)
            {
                softDeletableEntity.IsActive = false;
                _context.Set<T2>().Update(entity);
                return _context.SaveChangesAsync().ContinueWith(t => t.Result > 0);
            }
            return Task.FromResult(false);
        }
        public async Task<Commodity?> GetByIdWithIncludesAsync(int id, params Expression<Func<Commodity, object>>[] includes)
        {
            IQueryable<Commodity> query = _context.Commodities.Where(c => c.CommodityId == id && c.IsActive);
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task<Commodity?> GetByHotelNameAsync(string hotelName)
        {
            return await _context.Commodities
                .Include(c => c.Hotel)
                .FirstOrDefaultAsync(c => c.Hotel.Name == hotelName);
        }
    }
}