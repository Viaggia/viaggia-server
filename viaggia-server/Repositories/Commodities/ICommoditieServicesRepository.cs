
using viaggia_server.Models.Commodities;

namespace viaggia_server.Repositories.Commodities
{
    public interface ICommoditieServicesRepository : IRepository<CommoditieServices>
    {
        Task<IEnumerable<CommoditieServices>> GetAllAsync();
        Task<CommoditieServices?> GetByIdAsync(int id);
        Task<IEnumerable<CommoditieServices>> GetByCommoditieIdAsync(int commoditieId);
        Task<CommoditieServices> AddAsync(CommoditieServices entity);
        Task<CommoditieServices> UpdateAsync(CommoditieServices entity);
        Task<bool> SoftDeleteAsync(int id);

        Task<IEnumerable<CommoditieServices>> GetAllWithHotelAsync();

        Task<CommoditieServices?> GetCommoditieByHotelIdAsync(int hotelId);

    }
}
