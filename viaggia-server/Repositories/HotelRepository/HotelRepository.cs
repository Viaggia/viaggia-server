using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.DTOs.HotelFilterDTO;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Reviews;
using viaggia_server.Models.RoomTypeEnums;

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
                .FirstOrDefaultAsync(c => c.CommoditieId == commoditieId && c.IsActive);
        }

        public async Task<CommoditieServices> AddCommoditieServiceAsync(CommoditieServices commoditieService)
        {
            await _context.CommoditieServices.AddAsync(commoditieService);
            await _context.SaveChangesAsync();
            return commoditieService;
        }

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

        //public async Task<IEnumerable<Hotel>> FilterHotelsAsync(HotelFilterDTO filter)
        //{
        //    try
        //    {
        //        _logger.LogInformation("Filtering hotels with Commodities: {Commodities}, CommoditieServices: {CommoditieServices}, RoomTypes: {RoomTypes}",
        //            string.Join(", ", filter.Commodities), string.Join(", ", filter.CommoditieServices), string.Join(", ", filter.RoomTypes));

        //        var query = _context.Hotels
        //            .Where(h => h.IsActive)
        //            .Include(h => h.Commodities)
        //            .Include(h => h.CommoditieServices)
        //            .Include(h => h.RoomTypes)
        //            .AsQueryable();

        //        // Filter by Commodities (all specified commodities must be true)
        //        if (filter.Commodities != null && filter.Commodities.Any())
        //        {
        //            foreach (var commodity in filter.Commodities)
        //            {
        //                switch (commodity.ToLower())
        //                {
        //                    case "haswifi":
        //                        query = query.Where(h => h.Commodities.Any(c => c.IsActive && c.HasWiFi));
        //                        break;
        //                    case "haspool":
        //                        query = query.Where(h => h.Commodities.Any(c => c.IsActive && c.HasPool));
        //                        break;
        //                    case "hasgym":
        //                        query = query.Where(h => h.Commodities.Any(c => c.IsActive && c.HasGym));
        //                        break;
        //                    case "hasparking":
        //                        query = query.Where(h => h.Commodities.Any(c => c.IsActive && c.HasParking));
        //                        break;
        //                    case "hasbreakfast":
        //                        query = query.Where(h => h.Commodities.Any(c => c.IsActive && c.HasBreakfast));
        //                        break;
        //                    case "haslunch":
        //                        query = query.Where(h => h.Commodities.Any(c => c.IsActive && c.HasLunch));
        //                        break;
        //                    case "hasdinner":
        //                        query = query.Where(h => h.Commodities.Any(c => c.IsActive && c.HasDinner));
        //                        break;
        //                    case "hasspa":
        //                        query = query.Where(h => h.Commodities.Any(c => c.IsActive && c.HasSpa));
        //                        break;
        //                    case "hasairconditioning":
        //                        query = query.Where(h => h.Commodities.Any(c => c.IsActive && c.HasAirConditioning));
        //                        break;
        //                    case "hasaccessibilityfeatures":
        //                        query = query.Where(h => h.Commodities.Any(c => c.IsActive && c.HasAccessibilityFeatures));
        //                        break;
        //                    case "ispetfriendly":
        //                        query = query.Where(h => h.Commodities.Any(c => c.IsActive && c.IsPetFriendly));
        //                        break;
        //                    default:
        //                        _logger.LogWarning("Invalid commodity: {Commodity}", commodity);
        //                        break;
        //                }
        //            }
        //        }

        //        // Filter by CommoditieServices (all specified services must exist)
        //        if (filter.CommoditieServices != null && filter.CommoditieServices.Any())
        //        {
        //            foreach (var service in filter.CommoditieServices)
        //            {
        //                query = query.Where(h => h.CommoditieServices.Any(cs => cs.IsActive && cs.Name.ToLower() == service.ToLower()));
        //            }
        //        }

        //        // Filter by RoomTypes (all specified room types must exist)
        //        if (filter.RoomTypes != null && filter.RoomTypes.Any())
        //        {
        //            foreach (var roomType in filter.RoomTypes)
        //            {
        //                if (Enum.TryParse<RoomTypeEnum>(roomType, true, out var parsedRoomType))
        //                {
        //                    query = query.Where(h => h.RoomTypes.Any(rt => rt.IsActive && rt.Name == parsedRoomType));
        //                }
        //                else
        //                {
        //                    _logger.LogWarning("Invalid room type: {RoomType}", roomType);
        //                }
        //            }
        //        }

        //        var hotels = await query.ToListAsync();
        //        _logger.LogInformation("Found {Count} hotels matching filter criteria", hotels.Count);
        //        return hotels;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error filtering hotels");
        //        throw;
        //    }
        //}

        public async Task<IEnumerable<Hotel>> GetHotelsWithRelatedDataAsync()
        {
            try
            {
                _logger.LogInformation("Fetching hotels with related data");
                var hotels = await _context.Hotels
                    .Where(h => h.IsActive)
                    .Include(h => h.Commodities)
                    .Include(h => h.CommoditieServices)
                    .Include(h => h.RoomTypes)
                    .Include(h => h.Medias)
                    .Include(h => h.Reviews)
                    .Include(h => h.Packages)
                    .ToListAsync();
                _logger.LogInformation("Fetched {Count} hotels with related data", hotels.Count);
                return hotels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching hotels with related data");
                throw;
            }
        }

    }
}