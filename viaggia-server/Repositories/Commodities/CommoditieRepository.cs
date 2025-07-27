using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Commodities;

namespace viaggia_server.Repositories.Commodities
{
    public class CommoditieRepository : ICommoditieRepository
    {
        private readonly AppDbContext _context;

        public CommoditieRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Commoditie>> GetAllAsync()
        {
            return await _context.Commodities
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task<Commoditie?> GetByIdAsync(int id)
        {
            return await _context.Commodities
                .FirstOrDefaultAsync(c => c.CommoditieId == id && c.IsActive);
        }

        public async Task<Commoditie?> GetByHotelIdAsync(int hotelId)
        {
            return await _context.Commodities
                .Include(c => c.CommoditiesServices)
                .FirstOrDefaultAsync(c => c.HotelId == hotelId && c.IsActive);
        }

        public async Task<Commoditie> AddAsync(Commoditie entity)
        {
            await _context.Commodities.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Commoditie> UpdateAsync(Commoditie entity)
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

        Task<bool> ICommoditieRepository.UpdateAsync(Commoditie commoditie)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

