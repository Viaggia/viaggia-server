using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.DTOs.Reservation;
using viaggia_server.Models.Reservations;

namespace viaggia_server.Repositories.ReservationRepository
{
    public class ReservationRepository : Repository<Reservation>, IReservationRepository
    {
        public ReservationRepository(AppDbContext context) : base(context) {}

        public async Task<Reservation> CreateReservationAsync(Reservation reservation)
        {
            try
            {
                var result = await _context.Reservations.AddAsync(reservation);
                await _context.SaveChangesAsync();
                return result.Entity;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        } 

        public async Task<List<Reservation>> GetAllReservationsAsync()
        {
            try
            {
                var allReserves = await _context.Reservations.ToListAsync();
                if (allReserves == null) throw new Exception("Not found reserves");
                return allReserves;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<Reservation> GetReservationByIdAsync(int id)
        {
            try
            {
                var findReserve = await _context.Reservations.FirstOrDefaultAsync<Reservation>(r => r.ReservationId == id);
                if (findReserve.ReservationId == null)
                    throw new Exception("Not found this reserve");
                return findReserve;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
