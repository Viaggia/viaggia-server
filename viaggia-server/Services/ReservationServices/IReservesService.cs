using viaggia_server.DTOs.Reserve;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Users;

namespace viaggia_server.Services.Reservations
{
    public interface IReservesService
    {
        Task<List<ReserveDTO>> GetAllAsync();
        Task<ReserveDTO?> GetByIdAsync(int id);
        Task<ReserveDTO> CreateAsync(ReserveCreateDTO dto);
        Task<ReserveDTO> UpdateAsync(int id, ReserveUpdateDTO dto);
        Task<bool> SoftDeleteAsync(int id);
    }
}
