
using viaggia_server.Models.Commodities;
using viaggia_server.Models.CustomCommodities;

namespace viaggia_server.Repositories.CommodityRepository
{
    public interface ICustomCommodityRepository : IRepository<CustomCommodity>
    {
        Task<IEnumerable<CustomCommodity>> GetAllAsync();
        Task<CustomCommodity?> GetByIdAsync(int id);
        Task<IEnumerable<CustomCommodity>> GetByCommodityIdAsync(int commoditieId);
        Task<CustomCommodity> AddAsync(CustomCommodity entity);
        Task<CustomCommodity> UpdateAsync(CustomCommodity entity);
        Task<bool> SoftDeleteAsync(int id);

    }
}
