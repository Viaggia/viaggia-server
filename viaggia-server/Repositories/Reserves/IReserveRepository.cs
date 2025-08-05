using viaggia_server.Models.Reserves;

namespace viaggia_server.Repositories.Reserves
{
    public interface IReserveRepository
    {
        Task<IEnumerable<Reserve>> GetByHotelIdAsync(int hotelId);
        Task<Reserve?> GetByIdAsync(int id);
        Task<Reserve> UpdateAsync(Reserve reserve);
    }
}