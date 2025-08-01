using viaggia_server.Models.Reservations;

namespace viaggia_server.Repositories.Reservations
{
    public interface IReservationRepository
    {
        Task<Reservation?> GetReservationByIdAsync(int id);
    }

}
