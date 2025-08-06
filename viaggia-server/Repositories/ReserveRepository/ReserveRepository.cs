using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.DTOs.Reserves;
using viaggia_server.Models.Users;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Reserves;

namespace viaggia_server.Repositories.ReserveRepository
{
    public class ReserveRepository : IReserveRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReserveRepository> _logger;
        public ReserveRepository(AppDbContext context, ILogger<ReserveRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Reserve> CreateReserveAsync(Reserve reserve)
        {
            try
            {
                var result = await _context.Reserves.AddAsync(reserve);
                await _context.SaveChangesAsync();
                return result.Entity;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Reserve?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching reservation with ID: {ReserveId}", id);
                var reservation = await _context.Reserves
                    .Include(r => r.Hotel)
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.ReserveId == id && r.IsActive);
                if (reservation == null)
                    _logger.LogWarning("Reservation with ID: {ReserveId} not found or inactive", id);
                else
                    _logger.LogInformation("Reservation with ID: {ReserveId} fetched successfully", id);
                return reservation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching reservation with ID: {ReserveId}", id);
                throw;
            }
        }


        public async Task<IEnumerable<Reserve>> GetByHotelIdAsync(int hotelId)
        {
            try
            {
                _logger.LogInformation("Fetching reservations for HotelId: {HotelId}", hotelId);
                var reservations = await _context.Reserves
                    .Where(r => r.HotelId == hotelId && r.IsActive)
                    .Include(r => r.Hotel)
                    .Include(r => r.User)
                    .ToListAsync();
                _logger.LogInformation("Fetched {Count} reservations for HotelId: {HotelId}", reservations.Count, hotelId);
                return reservations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching reservations for HotelId: {HotelId}", hotelId);
                throw;
            }
        }
        public async Task<Reserve> UpdateAsync(Reserve reserve)
        {
            try
            {
                _logger.LogInformation("Updating reservation with ID: {ReserveId}", reserve.ReserveId);
                _context.Reserves.Update(reserve);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Reservation with ID: {ReserveId} updated successfully", reserve.ReserveId);
                return reserve;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reservation with ID: {ReserveId}", reserve.ReserveId);
                throw;
            }
        }

        public async Task<IEnumerable<Reserve>> GetReserveByUser(int userId)
        {
            try
            {
                var result = await _context.Reserves
                    .Where(r => r.UserId == userId && r.IsActive)
                    .Include(r => r.User)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<IEnumerable<Reserve>> GetByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Buscando reservas para o usuário com ID: {UserId}", userId);
                var reservations = await _context.Reserves
                    .Where(r => r.UserId == userId && r.IsActive)
                    .Include(r => r.Hotel)
                    .Include(r => r.User)
                    .ToListAsync();

                _logger.LogInformation("Encontradas {Count} reservas para o usuário com ID: {UserId}", reservations.Count, userId);
                return reservations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar reservas para o usuário com ID: {UserId}", userId);
                throw;
            }
        }
    }
}
