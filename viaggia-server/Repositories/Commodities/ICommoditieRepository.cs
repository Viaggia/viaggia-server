using System.Collections.Generic;
using System.Threading.Tasks;
using viaggia_server.Models.Commodities;


namespace viaggia_server.Repositories.Commodities
{
    public interface ICommoditieRepository : IRepository<Commoditie>
    {
 
        Task<Commoditie?> GetByHotelIdAsync(int hotelId);
       
    }
}
