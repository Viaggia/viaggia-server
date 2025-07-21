using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Package;

namespace viaggia_server.Controllers.PackagesController
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsTypesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoomsTypesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/RoomType
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomType>>> GetAll()
        {
            return await _context.RoomTypes
                .Include(r => r.PackageDateRooms)
                .ToListAsync();
        }

        // GET: api/RoomType/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomType>> GetById(int id)
        {
            var roomType = await _context.RoomTypes
                .Include(r => r.PackageDateRooms)
                .FirstOrDefaultAsync(r => r.RoomTypeId == id);

            if (roomType == null)
                return NotFound();

            return roomType;
        }

        // POST: api/RoomType
        [HttpPost]
        public async Task<ActionResult<RoomType>> Create(RoomType roomType)
        {
            _context.RoomTypes.Add(roomType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = roomType.RoomTypeId }, roomType);
        }

        // PUT: api/RoomType/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RoomType roomType)
        {
            if (id != roomType.RoomTypeId)
                return BadRequest();

            _context.Entry(roomType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.RoomTypes.Any(e => e.RoomTypeId == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/RoomType/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType == null)
                return NotFound();

            _context.RoomTypes.Remove(roomType);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
