using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;

namespace viaggia_server.Repositories
{
    public class PackageRepository : IPackageRepository
    {
        private readonly AppDbContext _context;

        public PackageRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<PackageDate>> GetPackageDatesAsync(int packageId)
        {
            return await _context.PackageDates
                .Where(pd => pd.PackageId == packageId)
                .ToListAsync();
        }

        public async Task<PackageDate?> GetPackageDateByIdAsync(int packageDateId)
        {
            return await _context.PackageDates
                .FirstOrDefaultAsync(pd => pd.PackageDateId == packageDateId);
        }

        public async Task<IEnumerable<Media>> GetPackageMediasAsync(int packageId)
        {
            return await _context.Medias
                .Where(m => m.PackageId == packageId)
                .ToListAsync();
        }

        public async Task<Media> AddMediaAsync(Media media)
        {
            await _context.Medias.AddAsync(media);
            return media;
        }

        public async Task<bool> DeleteMediaAsync(int mediaId)
        {
            var media = await _context.Medias.FindAsync(mediaId);
            if (media == null)
                return false;

            _context.Medias.Remove(media);
            await _context.SaveChangesAsync(); // Ensure changes are saved to the database  
            return true;
        }
    }
}