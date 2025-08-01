using System.Collections.Generic;
using System.Threading.Tasks;
using viaggia_server.Models.Commodities;


namespace viaggia_server.Repositories.Commodities
{
    public interface ICommoditieRepository : IRepository<Commoditie>
    {
        Task<IEnumerable<Commoditie>> GetAllAsync();
        Task<Commoditie?> GetByIdAsync(int id);
        Task<Commoditie?> GetByHotelIdAsync(int hotelId);
        Task<Commoditie> AddAsync(Commoditie entity);
        Task<Commoditie> UpdateAsync(Commoditie entity);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<bool> NameExistsAsync(string name);

    }
}
