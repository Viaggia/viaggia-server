using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;

namespace viaggia_server.Repositories.Hotels
{
    public class HotelRepository : IHotelRepository
    {
        private readonly AppDbContext _context;

        public HotelRepository(AppDbContext context)
        {
            _context = context;
        }
    }
}
