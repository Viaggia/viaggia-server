using viaggia_server.DTOs.ReservationDTO;
using viaggia_server.Models.Reservations;
using viaggia_server.Repositories.ReservationRepository;

namespace viaggia_server.Repositories.ReservationRepository
{
    public interface IReservationRepository : IRepository<Reservation>
    {
        Task<List<Reservation>> GetAllReservationsAsync();
        Task<Reservation> GetReservationByIdAsync(int id);
        Task<Reservation> CreateReservationAsync(Reservation reservation);
    }
}
