using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Hotels;
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
                .Where(pd => pd.PackageId == packageId && pd.IsActive)
                .ToListAsync();
        }

        public async Task<PackageDate?> GetPackageDateByIdAsync(int packageDateId)
        {
            return await _context.PackageDates
                .FirstOrDefaultAsync(pd => pd.PackageDateId == packageDateId && pd.IsActive);
        }

        public async Task<PackageDate> AddPackageDateAsync(PackageDate packageDate)
        {
            packageDate.IsActive = true;
            await _context.PackageDates.AddAsync(packageDate);
            await _context.SaveChangesAsync();
            return packageDate;
        }

        public async Task<IEnumerable<Media>> GetPackageMediasAsync(int packageId)
        {
            return await _context.Medias
                .Where(m => m.PackageId == packageId && m.IsActive)
                .ToListAsync();
        }

        public async Task<Media> AddMediaAsync(Media media)
        {
            await _context.Medias.AddAsync(media);
            await _context.SaveChangesAsync();
            return media;
        }

        public async Task<bool> DeleteMediaAsync(int mediaId)
        {
            var media = await _context.Medias.FindAsync(mediaId);
            if (media == null)
                return false;

            media.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Package>> SearchPackagesByDestinationAndDateAsync(string destination, DateTime startDate, DateTime endDate)
        {
            string destLower = destination.ToLower();

            return await _context.Packages
                .Include(p => p.Hotel)
                .Include(p => p.Medias)
                .Include(p => p.PackageDates)
                .Where(p => p.IsActive &&
                            (p.Destination.ToLower().Contains(destLower) ||
                             p.Hotel.Name.ToLower().Contains(destLower)) &&
                            p.PackageDates.Any(pd => pd.IsActive &&
                                                    pd.StartDate <= endDate &&
                                                    pd.EndDate >= startDate))
                .ToListAsync();
        }

        public async Task<bool> ReactivateAsync(int packageId)
        {
            var package = await _context.Packages
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.PackageId == packageId);

            if (package == null)
                return false;

            package.IsActive = true;

            // Reactivate associated PackageDates
            var packageDates = await _context.PackageDates
                .IgnoreQueryFilters()
                .Where(pd => pd.PackageId == packageId)
                .ToListAsync();
            foreach (var date in packageDates)
            {
                date.IsActive = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int?> GetHotelIdByNameAsync(string hotelName)
        {
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.Name.ToLower() == hotelName.ToLower() && h.IsActive);
            return hotel?.HotelId;
        }
    }
}