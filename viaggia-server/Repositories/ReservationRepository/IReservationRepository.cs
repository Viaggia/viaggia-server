using viaggia_server.DTOs.Reserves;
using viaggia_server.Models.Reserves;
using viaggia_server.Repositories.ReservesRepository;

namespace viaggia_server.Repositories.ReservesRepository
{
    public interface IReservesRepository : IRepository<Reserve>
    {

        Task<List<Reserve>> GetAllReservationsAsync();
        Task<Reserve> GetReservationByIdAsync(int id);
        Task<Reserve> CreateReservationAsync(Reserve reserves);
    }
}
