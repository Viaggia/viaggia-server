using viaggia_server.DTOs.Reserves;
using viaggia_server.Models.Reserves;

namespace viaggia_server.Repositories.ReserveRepository
{
    public interface IReserveRepository 
    {
        Task<Reserve?> GetByIdAsync(int id);
        Task<IEnumerable<Reserve>> GetByHotelIdAsync(int hotelId);
        Task<Reserve> CreateReserveAsync(Reserve reserve);
        Task<Reserve> UpdateAsync(Reserve reserve);


    }
}
