using viaggia_server.DTOs.Reserves;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Reserves;
using viaggia_server.Models.Users;

namespace viaggia_server.Services.Reserves
{
    public interface IReservesService
    {
        Task<List<Reserve>> GetAllAsync();
        Task<Reserve?> GetByIdAsync(int id);
        Task<Reserve> CreateAsync(ReservesCreateDTO dto);
        Task<Reserve> UpdateAsync(int id, ReservationUpdateDTO dto);
        Task<bool> SoftDeleteAsync(int id);
    }
}
