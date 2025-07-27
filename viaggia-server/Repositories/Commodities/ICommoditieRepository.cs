using System.Collections.Generic;
using System.Threading.Tasks;
using viaggia_server.Models.Commodities;


namespace viaggia_server.Repositories.Commodities
{
    public interface ICommoditieRepository
    {
        Task<Commoditie> AddAsync(Commoditie commoditie);
        Task<Commoditie?> GetByIdAsync(int id);
        Task<IEnumerable<Commoditie>> GetAllAsync();
        Task<Commoditie?> GetByHotelIdAsync(int hotelId);
        Task<bool> UpdateAsync(Commoditie commoditie);
        Task<bool> DeleteAsync(int id);
    }
}
