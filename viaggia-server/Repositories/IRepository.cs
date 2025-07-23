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
        Task<bool> SoftDeleteAsync<T1>(int id) where T1 : class, ISoftDeletable;
    }
}