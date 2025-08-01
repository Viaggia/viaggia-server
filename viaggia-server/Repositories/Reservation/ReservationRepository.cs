using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Reservations;

namespace viaggia_server.Repositories.Reservations
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly AppDbContext _context;

        public ReservationRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Reservation?> GetReservationByIdAsync(int id)
        {
            return await _context.Reservations
                .Where(r => r.IsActive && r.ReservationId == id)
                .FirstOrDefaultAsync();
        }
    }

}
