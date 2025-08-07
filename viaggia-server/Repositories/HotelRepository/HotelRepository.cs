using Microsoft.EntityFrameworkCore;
using Stripe;
using viaggia_server.Data;
using viaggia_server.DTOs.Hotel;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.CustomCommodities;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Reserves;
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

        public async Task UpdateAsync(Hotel hotel)
        {
            _context.Hotels.Update(hotel);
            await _context.SaveChangesAsync();
        }

        public async Task<Hotel?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching hotel with ID: {HotelId}", id);
                var hotel = await _context.Hotels
                    .Include(h => h.Commodities)
                    .Include(h => h.CustomCommodities)
                    .Include(h => h.RoomTypes)
                    .Include(h => h.Medias)
                    .Include(h => h.Reviews)
                    .Include(h => h.Packages)
                    .FirstOrDefaultAsync(h => h.HotelId == id && h.IsActive);
                if (hotel == null)
                    _logger.LogWarning("Hotel with ID: {HotelId} not found or inactive", id);
                else
                    _logger.LogInformation("Hotel with ID: {HotelId} fetched successfully", id);
                return hotel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching hotel with ID: {HotelId}", id);
                throw;
            }
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
        public async Task UpdateRoomTypeAsync(HotelRoomType roomType)
        {
            _context.RoomTypes.Update(roomType);
            await _context.SaveChangesAsync();
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

        public async Task<Models.Reviews.Review> AddReviewAsync(Models.Reviews.Review review)
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<IEnumerable<Models.Reviews.Review>> GetReviewsByHotelIdAsync(int hotelId)
        {
            return await _context.Reviews
                .Where(r => r.HotelId == hotelId && r.IsActive)
                .ToListAsync();
        }

        public async Task<Models.Reviews.Review?> GetReviewByIdAsync(int reviewId)
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

        public async Task<Commodity> AddCommodityAsync(Commodity commodity)
        {
            await _context.Commodities.AddAsync(commodity);
            await _context.SaveChangesAsync();
            return commodity;
        }

        public async Task<IEnumerable<CustomCommodity>> GetCustomCommodityByHotelIdAsync(int hotelId)
        {
            return await _context.CustomCommodities
                .Where(cs => cs.HotelId == hotelId && cs.IsActive)
                .ToListAsync();
        }

        public async Task<Commodity?> GetCommodityByIdAsync(int commodityId)
        {
            return await _context.Commodities
                .FirstOrDefaultAsync(c => c.CommodityId == commodityId && c.IsActive);
        }

        public async Task<CustomCommodity> AddCustomCommodityAsync(CustomCommodity CustomCommodity)
        {
            await _context.CustomCommodities.AddAsync(CustomCommodity);
            await _context.SaveChangesAsync();
            return CustomCommodity;
        }



        public async Task<CustomCommodity?> GetCustomCommodityByIdAsync(int customCommodityId)
        {
            return await _context.CustomCommodities
                .FirstOrDefaultAsync(cs => cs.CustomCommodityId == customCommodityId && cs.IsActive);
        }

        public async Task<IEnumerable<Hotel>> GetHotelsWithRelatedDataAsync()
        {
            try
            {
                _logger.LogInformation("Fetching hotels with related data");
                var hotels = await _context.Hotels
                    .Where(h => h.IsActive)
                    .Include(h => h.Commodities)
                    .Include(h => h.CustomCommodities)
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

        public async Task<IEnumerable<HotelRoomType>> GetAvailableRoomTypesAsync(int hotelId, int numberOfPeople, DateTime checkInDate, DateTime checkOutDate)
        {
            try
            {
                _logger.LogInformation("Fetching available room types for HotelId: {HotelId}, People: {NumberOfPeople}, CheckIn: {CheckInDate}, CheckOut: {CheckOutDate}",
                    hotelId, numberOfPeople, checkInDate, checkOutDate);

                // Fetch room types with sufficient capacity
                var roomTypes = await _context.RoomTypes
                    .Where(rt => rt.HotelId == hotelId && rt.IsActive && rt.Capacity >= numberOfPeople)
                    .ToListAsync();

                // Fetch reservations that overlap with the requested dates
                var reservations = await _context.Reserves
                    .Where(r => r.HotelId == hotelId && r.IsActive &&
                                (checkInDate <= r.CheckOutDate && checkOutDate >= r.CheckInDate))
                    .ToListAsync();

                // Calculate available rooms for each room type
                var availableRoomTypes = new List<HotelRoomType>();
                foreach (var roomType in roomTypes)
                {
                    var reservedRooms = reservations
                        .SelectMany(r => r.ReserveRooms)
                        .Where(rr => rr.RoomTypeId == roomType.RoomTypeId)
                        .Sum(rr => rr.Quantity);

                    var availableRooms = roomType.TotalRooms - reservedRooms;

                    if (availableRooms > 0)
                    {
                        roomType.AvailableRooms = availableRooms;
                        availableRoomTypes.Add(roomType);
                    }
                }


                _logger.LogInformation("Found {Count} available room types for HotelId: {HotelId}", availableRoomTypes.Count, hotelId);
                return availableRoomTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available room types for HotelId: {HotelId}", hotelId);
                throw;
            }
        }


        public async Task<IEnumerable<Commodity>> GetCommodityByHotelIdAsync(int hotelId)
        {
            try
            {
                _logger.LogInformation("Fetching commodities for HotelId: {HotelId}", hotelId);
                var commodities = await _context.Commodities
                    .Where(c => c.HotelId == hotelId && c.IsActive)
                    .ToListAsync();
                _logger.LogInformation("Fetched {Count} commodities for HotelId: {HotelId}", commodities.Count, hotelId);
                return commodities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching commodities for HotelId: {HotelId}", hotelId);
                throw;
            }
        }

        public async Task<IEnumerable<Hotel>> GetAvailableHotelsByDestinationAsync(
         string city,
         int numberOfPeople,
         int numberOfRooms,
         DateTime checkInDate,
         DateTime checkOutDate)
        {
            try
            {
                _logger.LogInformation(
                    "Fetching available hotels for City: {City}, People: {NumberOfPeople}, Rooms: {NumberOfRooms}, CheckIn: {CheckInDate}, CheckOut: {CheckOutDate}",
                    city, numberOfPeople, numberOfRooms, checkInDate, checkOutDate);

                // Fetch hotels in the specified city
                var hotels = await _context.Hotels
                    .Where(h => h.IsActive && h.City.ToLower() == city.ToLower())
                    .Include(h => h.RoomTypes.Where(rt => rt.IsActive && rt.Capacity >= numberOfPeople))
                    .Include(h => h.Commodities)
                    .Include(h => h.CustomCommodities)
                    .Include(h => h.Medias)
                    .Include(h => h.Reviews)
                    .Include(h => h.Packages)
                    .ToListAsync();

                // Fetch reservations that overlap with the requested dates
                var reservations = await _context.Reserves
                    .Where(r => r.IsActive &&
                                r.HotelId.HasValue &&
                                hotels.Select(h => h.HotelId).Contains(r.HotelId.Value) &&
                                (checkInDate <= r.CheckOutDate && checkOutDate >= r.CheckInDate))
                    .ToListAsync();

                // Filter hotels with sufficient available rooms
                var availableHotels = new List<Hotel>();
                foreach (var hotel in hotels)
                {
                    var availableRoomTypes = new List<HotelRoomType>();
                    foreach (var roomType in hotel.RoomTypes)
                    {
                        // Count reserved rooms for this room type in the date range
                        var reservedRooms = reservations
                            .SelectMany(r => r.ReserveRooms)
                            .Where(rr => rr.RoomTypeId == roomType.RoomTypeId)
                            .Sum(rr => rr.Quantity);


                        var availableRooms = roomType.TotalRooms - reservedRooms;
                        if (availableRooms >= numberOfRooms)
                        {
                            roomType.AvailableRooms = availableRooms;
                            availableRoomTypes.Add(roomType);
                        }
                    }

                    if (availableRoomTypes.Any())
                    {
                        hotel.RoomTypes = availableRoomTypes;
                        availableHotels.Add(hotel);
                    }
                }

                _logger.LogInformation("Found {Count} available hotels in {City}", availableHotels.Count, city);
                return availableHotels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available hotels for City: {City}", city);
                throw;
            }
        }
        public async Task<Hotel?> GetHotelByIdWithDetailsAsync(int hotelId)
        {
            return await _context.Hotels
                .Include(h => h.RoomTypes)
                .Include(h => h.Medias)
                .Include(h => h.Reviews)
                .Include(h => h.Packages)
                .Include(h => h.Commodities)
                .Include(h => h.CustomCommodities)
                .FirstOrDefaultAsync(h => h.HotelId == hotelId);
        }


        public async Task<IEnumerable<Hotel>> GetHotelsByUserIdAsync(int userId)
        {
            return await _context.Hotels
                .Where(h => h.UserId == userId && h.IsActive)
                .Include(h => h.RoomTypes)
                .Include(h => h.Medias)
                .Include(h => h.Reviews)
                .Include(h => h.Packages)
                .Include(h => h.Commodities)
                .Include(h => h.CustomCommodities)
                .ToListAsync();
        }
        public async Task<Hotel> GetByIdHotel(int Hotel)
        {
            try
            {
                var hotelId = await _context.Hotels.FindAsync(Hotel);
                _logger.LogInformation($"Hotel: {Hotel}");
                return hotelId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<Reserve>> GetReservationsByHotelIdAsync(int hotelId)
        {
            return await _context.Reserves
                .Where(r => r.HotelId == hotelId && r.IsActive)
                .Include(r => r.User)
                .Include(r => r.Hotel)
                .ToListAsync();
        }

            public async Task<List<HotelBalanceDTO>> GetBalancesHotelsAsync()
            {
                try
                {
                    var chargeService = new ChargeService();
                    var options = new ChargeListOptions
                    {
                        Limit = 100,
                    };

                    // Auto-paging to retrieve all charges
                    var allCharges = chargeService.ListAutoPagingAsync(options);

                    var hotelPayments = new Dictionary<int, long>();

                    await foreach (var charge in allCharges)
                    {
                        if (!charge.Paid || !charge.Metadata.ContainsKey("hotelId"))
                            continue;

                        if (int.TryParse(charge.Metadata["hotelId"], out var hotelId))
                        {
                            if (!hotelPayments.ContainsKey(hotelId))
                                hotelPayments[hotelId] = 0;

                            hotelPayments[hotelId] += charge.Amount;
                        }
                    }

                    var hotelIds = hotelPayments.Keys.ToList();

                    var hoteis = await _context.Hotels
                        .Where(h => hotelIds.Contains(h.HotelId))
                        .ToListAsync();

                    var resultado = hoteis.Select(h => new HotelBalanceDTO
                    {
                        HotelName = h.Name,
                        TotalBalance = hotelPayments.ContainsKey(h.HotelId) ? hotelPayments[h.HotelId] / 100.0 : 0
                    }).ToList();
                _logger.LogInformation("resultado: {resultado}", resultado);
                    return resultado;
                }
                catch (Exception ex)
                {
                    throw new Exception("Erro ao buscar os saldos dos hotéis: " + ex.Message, ex);
                }
            }


    }
}
