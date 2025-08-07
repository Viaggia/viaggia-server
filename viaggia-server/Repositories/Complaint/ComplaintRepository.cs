using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models;

namespace viaggia_server.Repositories
{
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly AppDbContext _context;

        public ComplaintRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Complaint> CreateComplaintAsync(Complaint complaint)
        {
            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();
            return complaint;
        }

        public async Task<List<Complaint>> GetComplaintsByHotelIdAsync(int hotelId)
        {
            return await _context.Complaints
                .Where(c => c.HotelId == hotelId && c.IsActive)
                .Include(c => c.User)
                .ToListAsync();
        }
    }
}
