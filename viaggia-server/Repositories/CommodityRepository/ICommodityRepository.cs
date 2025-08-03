using viaggia_server.Models.Commodities;

namespace viaggia_server.Repositories.CommodityRepository
{
    public interface ICommodityRepository : IRepository<Commodity>
    {
        Task<IEnumerable<Commodity>> GetAllAsync();
        Task<Commodity?> GetByIdAsync(int id);
        Task<Commodity?> GetByHotelIdAsync(int hotelId);
        Task<Commodity?> GetByHotelNameAsync(string hotelName);
        Task<Commodity> AddAsync(Commodity entity);
        Task<Commodity> UpdateAsync(Commodity entity);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> DeleteAsync(int id);

    }
}