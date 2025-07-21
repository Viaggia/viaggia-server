using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Package;

namespace viaggia_server.Controllers.PackagesController
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackagesDatesRoomsTypesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PackagesDatesRoomsTypesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/PackageDateRoomType
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PackageDateRoomType>>> GetAll()
        {
            return await _context.PackageDateRoomTypes
                .Include(pdrt => pdrt.PackageDate)
                .Include(pdrt => pdrt.RoomType)
                .ToListAsync();
        }

        // GET: api/PackageDateRoomType/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PackageDateRoomType>> GetById(int id)
        {
            var item = await _context.PackageDateRoomTypes
                .Include(pdrt => pdrt.PackageDate)
                .Include(pdrt => pdrt.RoomType)
                .FirstOrDefaultAsync(pdrt => pdrt.PackageDateRoomTypeId == id);

            if (item == null)
                return NotFound();

            return item;
        }

        // POST: api/PackageDateRoomType
        [HttpPost]
        public async Task<ActionResult<PackageDateRoomType>> Create(PackageDateRoomType item)
        {
            _context.PackageDateRoomTypes.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = item.PackageDateRoomTypeId }, item);
        }

        // PUT: api/PackageDateRoomType/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PackageDateRoomType item)
        {
            if (id != item.PackageDateRoomTypeId)
                return BadRequest();

            _context.Entry(item).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.PackageDateRoomTypes.Any(e => e.PackageDateRoomTypeId == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/PackageDateRoomType/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.PackageDateRoomTypes.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.PackageDateRoomTypes.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
