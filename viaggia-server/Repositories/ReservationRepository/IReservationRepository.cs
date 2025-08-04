using viaggia_server.DTOs.Reservation;
using viaggia_server.Models.Reserves;
using viaggia_server.Repositories.ReservationRepository;

namespace viaggia_server.Repositories.ReservationRepository
{
    public interface IReservationRepository : IRepository<Reserve>
    {

        Task<List<Reserve>> GetAllReservationsAsync();
        Task<Reserve> GetReservationByIdAsync(int id);
        Task<Reserve> CreateReservationAsync(Reserve reservation);
    }
}
