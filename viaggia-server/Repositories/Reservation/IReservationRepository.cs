using viaggia_server.Models.Reserves;

namespace viaggia_server.Repositories.Reserves
{
    public interface IReservationRepository
    {
        Task<Reserve?> GetReservationByIdAsync(int id);
    }

}
