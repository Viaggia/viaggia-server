using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Reviews;

namespace viaggia_server.Repositories.HotelRepository
{
    public class HotelRepository : IHotelRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HotelRepository> _logger;

        public HotelRepository(AppDbContext context, ILogger<HotelRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<HotelRoomType> AddRoomTypeAsync(HotelRoomType roomType)
        {
            try
            {
                _logger.LogInformation("Adding room type: {Name}, HotelId: {HotelId}, TotalRooms: {TotalRooms}",
                    roomType.Name, roomType.HotelId, roomType.TotalRooms);
                await _context.RoomTypes.AddAsync(roomType);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Room type added successfully: {RoomTypeId}", roomType.RoomTypeId);
                return roomType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding room type for HotelId: {HotelId}", roomType.HotelId);
                throw;
            }
        }

        // Outros métodos permanecem iguais
        public async Task<bool> NameExistsAsync(string name)
        {
            return await _context.Hotels.AnyAsync(h => h.Name.ToLower() == name.ToLower() && h.IsActive);
        }

        public async Task<bool> CnpjExistsAsync(string? cnpj)
        {
            if (string.IsNullOrEmpty(cnpj))
                return false;
            return await _context.Hotels.AnyAsync(h => h.Cnpj.ToLower() == cnpj.ToLower() && h.IsActive);
        }

        public async Task<Hotel?> GetHotelByNameAsync(string name)
        {
            return await _context.Hotels
                .FirstOrDefaultAsync(h => h.Name.ToLower() == name.ToLower() && h.IsActive);
        }

        public async Task<IEnumerable<HotelRoomType>> GetHotelRoomTypesAsync(int hotelId)
        {
            return await _context.RoomTypes
                .Where(rt => rt.HotelId == hotelId && rt.IsActive)
                .ToListAsync();
        }

        public async Task<HotelRoomType?> GetRoomTypeByIdAsync(int roomTypeId)
        {
            return await _context.RoomTypes
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId && rt.IsActive);
        }

        public async Task<bool> UpdateRoomAvailabilityAsync(int roomTypeId, int roomsToReserve)
        {
            var roomType = await _context.RoomTypes
                .FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId && rt.IsActive);
            if (roomType == null || roomType.AvailableRooms < roomsToReserve)
                return false;

            roomType.AvailableRooms -= roomsToReserve;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Media> AddMediaAsync(Media media)
        {
            await _context.Medias.AddAsync(media);
            await _context.SaveChangesAsync();
            return media;
        }

        public async Task<IEnumerable<Media>> GetMediasByHotelIdAsync(int hotelId)
        {
            return await _context.Medias
                .Where(m => m.HotelId == hotelId && m.IsActive)
                .ToListAsync();
        }

        public async Task<Media?> GetMediaByIdAsync(int mediaId)
        {
            return await _context.Medias
                .FirstOrDefaultAsync(m => m.MediaId == mediaId && m.IsActive);
        }

        public async Task<bool> SoftDeleteMediaAsync(int mediaId)
        {
            var media = await _context.Medias.FindAsync(mediaId);
            if (media != null)
            {
                media.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<Review> AddReviewAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<IEnumerable<Review>> GetReviewsByHotelIdAsync(int hotelId)
        {
            return await _context.Reviews
                .Where(r => r.HotelId == hotelId && r.IsActive)
                .ToListAsync();
        }

        public async Task<Review?> GetReviewByIdAsync(int reviewId)
        {
            return await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.IsActive);
        }

        public async Task<Package> AddPackageAsync(Package package)
        {
            await _context.Packages.AddAsync(package);
            await _context.SaveChangesAsync();
            return package;
        }

        public async Task<IEnumerable<Package>> GetPackagesByHotelIdAsync(int hotelId)
        {
            return await _context.Packages
                .Where(p => p.HotelId == hotelId && p.IsActive)
                .ToListAsync();
        }

        public async Task<Package?> GetPackageByIdAsync(int packageId)
        {
            return await _context.Packages
                .FirstOrDefaultAsync(p => p.PackageId == packageId && p.IsActive);
        }

        public async Task<Commoditie> AddCommodityAsync(Commoditie commoditie)
        {
            await _context.Commodities.AddAsync(commoditie);
            await _context.SaveChangesAsync();
            return commoditie;
        }

        public async Task<IEnumerable<Commoditie>> GetCommoditiesByHotelIdAsync(int hotelId)
        {
            return await _context.Commodities
                .Where(c => c.HotelId == hotelId && c.IsActive)
                .ToListAsync();
        }

        public async Task<Commoditie?> GetCommodityByIdAsync(int commoditieId)
        {
            return await _context.Commodities
                .Include(c => c.CommoditieServices)
                .FirstOrDefaultAsync(c => c.CommoditieId == commoditieId && c.IsActive);
        }

        public async Task<CommoditieServices> AddCommoditieServiceAsync(CommoditieServices commoditieService)
        {
            await _context.CommoditieServices.AddAsync(commoditieService);
            await _context.SaveChangesAsync();
            return commoditieService;
        }

        public async Task<IEnumerable<CommoditieServices>> GetCommoditieServicesByHotelIdAsync(int hotelId)
        public async Task<IEnumerable<CommoditieServices>> GetCommoditieServicesByHotelIdAsync(int hotelId)
        {
            return await _context.CommoditieServices
                .Where(cs => cs.HotelId == hotelId && cs.IsActive)
                .ToListAsync();
        }

        public async Task<CommoditieServices?> GetCommoditieServiceByIdAsync(int commoditieServiceId)
        {
            return await _context.CommoditieServices
                .FirstOrDefaultAsync(cs => cs.CommoditieServicesId == commoditieServiceId && cs.IsActive);
        }
    }
}