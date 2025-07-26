using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Hotels;
using viaggia_server.Repositories;
using viaggia_server.Repositories.Hotel;

namespace viaggia_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IRepository<Hotel> _genericRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IWebHostEnvironment _environment;

        public HotelsController(
            AppDbContext context,
            IRepository<Hotel> genericRepository,
            IHotelRepository hotelRepository,
            IWebHostEnvironment environment)
        {
            _context = context;
            _genericRepository = genericRepository;
            _hotelRepository = hotelRepository;
            _environment = environment;
        }

        // GET: api/Hotels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Hotel>>> GetHotels()
        {
            var hotels = await _context.Hotels
                .Include(h => h.Address)
                .Include(h => h.RoomTypes)
                .Include(h => h.Medias)
                .Include(h => h.Reviews)
                .Include(h => h.Packages)
                .Include(h => h.HotelDates)
                .Include(h => h.Reservations)
                .ToListAsync();

            return Ok(hotels);
        }

        // GET: api/Hotels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Hotel>> GetHotel(int id)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Address)
                .Include(h => h.RoomTypes)
                .Include(h => h.Medias)
                .Include(h => h.Reviews)
                .Include(h => h.Packages)
                .Include(h => h.HotelDates)
                .Include(h => h.Reservations)
                .FirstOrDefaultAsync(h => h.HotelId == id);

            if (hotel == null)
                return NotFound();

            return Ok(hotel);
        }

        // POST: api/Hotels
        [HttpPost]
        public async Task<ActionResult<Hotel>> CreateHotel([FromBody] Hotel hotel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _genericRepository.AddAsync(hotel);
            return CreatedAtAction(nameof(GetHotel), new { id = created.HotelId }, created);
        }

        // PUT: api/Hotels/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] Hotel hotel)
        {
            if (id != hotel.HotelId)
                return BadRequest("Hotel ID mismatch");

            var existing = await _genericRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            await _genericRepository.UpdateAsync(hotel);
            return NoContent();
        }

        // DELETE: api/Hotels/5 (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var existing = await _genericRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            existing.IsActive = false;
            await _genericRepository.UpdateAsync(existing);
            return NoContent();
        }

        // GET: api/Hotels/5/address
        [HttpGet("{id}/address")]
        public async Task<ActionResult> GetHotelAddress(int id)
        {
            var address = await _hotelRepository.GetAddressByHotelIdAsync(id);
            if (address == null)
                return NotFound("Address not found for this hotel.");

            return Ok(address);
        }
    }
}