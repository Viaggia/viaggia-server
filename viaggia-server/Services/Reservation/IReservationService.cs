using viaggia_server.DTOs.Reservation;

namespace viaggia_server.Services.Reservations
{
    public interface IReservationService
    {
        Task<List<ReservationDTO>> GetAllAsync();
        Task<ReservationDTO?> GetByIdAsync(int id);
        Task<ReservationDTO> CreateAsync(ReservationCreateDTO dto);
        Task<ReservationDTO> UpdateAsync(int id, ReservationUpdateDTO dto);
        Task<bool> SoftDeleteAsync(int id);
    }
}
