using viaggia_server.Models.Commodities;

namespace viaggia_server.Repositories
{
    public interface IRepository<T> where T : class, ISoftDeletable
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> SaveChangesAsync();
        Task<T2?> GetByIdAsync<T2>(int id) where T2 : class, ISoftDeletable;
        Task<bool> SoftDeleteAsync<T2>(int id) where T2 : class, ISoftDeletable; // New method for cross-entity soft delete
    }
}