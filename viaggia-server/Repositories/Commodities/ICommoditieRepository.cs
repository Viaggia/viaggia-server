using System.Collections.Generic;
using System.Threading.Tasks;
using viaggia_server.Models.Commodities;


namespace viaggia_server.Repositories.Commodities
{
    public interface ICommoditieRepository
    {
        Task<Commodity> AddAsync(Commodity commoditie);
        Task<Commodity?> GetByIdAsync(int id);
        Task<IEnumerable<Commodity>> GetAllAsync();
        Task<Commodity?> GetByHotelIdAsync(int hotelId);
        Task<bool> UpdateAsync(Commodity commoditie);
        Task<bool> DeleteAsync(int id);
    }
}
