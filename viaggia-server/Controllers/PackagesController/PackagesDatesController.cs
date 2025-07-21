using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Package;

namespace viaggia_server.Controllers.PackagesController
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackagesDatesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PackagesDatesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/PackageDate
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PackageDate>>> GetAll()
        {
            return await _context.PackageDates
                .Include(d => d.Package)
                .Include(d => d.PackageDateRoomTypes)
                    .ThenInclude(r => r.RoomType)
                .ToListAsync();
        }

        // GET: api/PackageDate/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PackageDate>> GetById(int id)
        {
            var packageDate = await _context.PackageDates
                .Include(d => d.Package)
                .Include(d => d.PackageDateRoomTypes)
                    .ThenInclude(r => r.RoomType)
                .FirstOrDefaultAsync(d => d.PackageDateId == id);

            if (packageDate == null)
                return NotFound();

            return packageDate;
        }

        // POST: api/PackageDate
        [HttpPost]
        public async Task<ActionResult<PackageDate>> Create(PackageDate packageDate)
        {
            _context.PackageDates.Add(packageDate);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = packageDate.PackageDateId }, packageDate);
        }

        // PUT: api/PackageDate/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PackageDate packageDate)
        {
            if (id != packageDate.PackageDateId)
                return BadRequest();

            _context.Entry(packageDate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.PackageDates.Any(e => e.PackageDateId == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/PackageDate/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var packageDate = await _context.PackageDates.FindAsync(id);
            if (packageDate == null)
                return NotFound();

            _context.PackageDates.Remove(packageDate);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
