using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.DTOs.Reservation;
using viaggia_server.Models.Reserves;

namespace viaggia_server.Repositories.ReservationRepository
{
    public class ReservationRepository : Repository<Reserve>, IReservationRepository
    {
        public ReservationRepository(AppDbContext context) : base(context) {}

        public async Task<Reserve> CreateReservationAsync(Reserve reservation)
        {
            try
            {
                var result = await _context.Reserves.AddAsync(reservation);
                await _context.SaveChangesAsync();
                return result.Entity;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        } 

        public async Task<List<Reserve>> GetAllReservationsAsync()
        {
            try
            {
                var allReserves = await _context.Reserves.ToListAsync();
                if (allReserves == null) throw new Exception("Not found reserves");
                return allReserves;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<Reserve> GetReservationByIdAsync(int id)
        {
            try
            {
                var findReserve = await _context.Reserves.FirstOrDefaultAsync<Reserve>(r => r.ReserveId == id);
                if (findReserve.ReserveId == null)
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
