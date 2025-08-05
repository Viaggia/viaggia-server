using viaggia_server.DTOs.Reserve;
using viaggia_server.Models.Reserves;
using viaggia_server.Repositories.ReservationRepository;

namespace viaggia_server.Repositories.ReservationRepository
{
    public interface IReserveRepository 
    {
        Task<Reserve?> GetByIdAsync(int id);
        Task<IEnumerable<Reserve>> GetByHotelIdAsync(int hotelId);
        Task<Reserve> CreateReserveAsync(Reserve reserve);
        Task<Reserve> UpdateAsync(Reserve reserve);


    }
}
