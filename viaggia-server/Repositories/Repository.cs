using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;

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
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                return false;

            entity.IsActive = false;
            _context.Set<T>().Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SoftDeleteAsync<T2>(int id) where T2 : class, ISoftDeletable
        {
            var entityType = typeof(T2);
            var primaryKey = _context.Model.FindEntityType(entityType)
                ?.FindPrimaryKey()
                ?.Properties
                .FirstOrDefault()
                ?.Name;

            if (primaryKey == null)
                throw new InvalidOperationException($"Primary key not found for entity type {entityType.Name}");

            var entity = await _context.Set<T2>()
                .FirstOrDefaultAsync(e => EF.Property<int>(e, primaryKey) == id && e.IsActive);

            if (entity == null)
                return false;

            entity.IsActive = false;
            _context.Set<T2>().Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            var entityType = typeof(T);
            var primaryKey = _context.Model.FindEntityType(entityType)
                ?.FindPrimaryKey()
                ?.Properties
                .FirstOrDefault()
                ?.Name;

            if (primaryKey == null)
                throw new InvalidOperationException($"Primary key not found for entity type {entityType.Name}");

            return await _context.Set<T>()
                .FirstOrDefaultAsync(e => EF.Property<int>(e, primaryKey) == id && e.IsActive);
        }

        public async Task<T2?> GetByIdAsync<T2>(int id) where T2 : class, ISoftDeletable
        {
            var entityType = typeof(T2);
            var primaryKey = _context.Model.FindEntityType(entityType)
                ?.FindPrimaryKey()
                ?.Properties
                .FirstOrDefault()
                ?.Name;

            if (primaryKey == null)
                throw new InvalidOperationException($"Primary key not found for entity type {entityType.Name}");

            return await _context.Set<T2>()
                .FirstOrDefaultAsync(e => EF.Property<int>(e, primaryKey) == id && e.IsActive);
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
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}