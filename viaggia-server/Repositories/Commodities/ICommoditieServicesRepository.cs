using viaggia_server.Models.Commodities;

namespace viaggia_server.Repositories.Commodities
{
    public interface ICommoditieServicesRepository
    {
        
        Task<IEnumerable<CommoditieServices>> GetByCommoditieIdAsync(int commoditieId);
     

    }
}
