using viaggia_server.DTOs.ReservationDTO;

namespace viaggia_server.Services.ReservationServices
{
    public interface IReservationServices
    {
        Task<CreateReservation> CreateReservationAsync(CreateReservation createReservation);
        Task<CreateReservation> GetReservationByIdAsync(string reservationId);
        Task<IEnumerable<CreateReservation>> GetAllReservationsAsync();

    }
}
