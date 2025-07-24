using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models;

namespace viaggia_server.Repositories
{
    public class Repository<T> : IRepository<T> where T : class, ISoftDeletable
    {
        private readonly AppDbContext _context;

        public Repository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<T> AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _context.Set<T>().AddAsync(entity);
            return entity;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity == null)
                return false;

            entity.IsActive = false;
            _context.Set<T>().Update(entity);
            return true;
        }

        public async Task<bool> SoftDeleteAsync<T1>(int id) where T1 : class, ISoftDeletable
        {
            var entity = await _context.Set<T1>().FindAsync(id);
            if (entity == null)
                return false;

            entity.IsActive = false;
            _context.Set<T1>().Update(entity);
            return true;
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity == null || !entity.IsActive)
                return null;

            return entity;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>()
                .Where(e => e.IsActive)
                .ToListAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.Set<T>().Update(entity);
            return entity;
        }
    }
}