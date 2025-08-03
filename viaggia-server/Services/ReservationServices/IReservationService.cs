using viaggia_server.DTOs.Reservation;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Users;

namespace viaggia_server.Services.Reservations
{
    public interface IReservationService
    {
        Task<List<ReservationDTO>> GetAllAsync();
        Task<ReservationDTO?> GetByIdAsync(int id);
        //Task<ReservationDTO> CreateAsync(ReservationCreateDTO dto, Hotel hotel, User user);
        Task<ReservationDTO> CreateAsync(ReservationCreateDTO dto);
        Task<ReservationDTO> UpdateAsync(int id, ReservationUpdateDTO dto);
        Task<bool> SoftDeleteAsync(int id);
    }
}
