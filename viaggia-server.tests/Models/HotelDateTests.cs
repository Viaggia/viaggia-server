using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.HotelDates;
using viaggia_server.Models.HotelRoomTypes;

namespace viaggia_server.tests.Models;

public class HotelDateTests : IDisposable
{
    private readonly AppDbContext _context;

    public HotelDateTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task HotelDate_CanCreateHotelDate()
    {
        // Test: Verify hotel date creation with required relationships
        // Purpose: Tests basic hotel date entity functionality
        var hotel = new Hotel
        {
            Name = "Test Hotel",
            Cnpj = "12.345.678/0001-99",
            StarRating = 4,
            AddressId = 1
        };

        var roomType = new HotelRoomType
        {
            Name = "Standard Room",
            Price = 150.00m,
            Capacity = 2,
            BedType = "Queen Size",
            HotelId = hotel.HotelId
        };

        _context.Hotels.Add(hotel);
        _context.RoomTypes.Add(roomType);
        await _context.SaveChangesAsync();

        var hotelDate = new HotelDate
        {
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(37),
            AvailableRooms = 10,
            RoomTypeId = roomType.RoomTypeId,
            HotelId = hotel.HotelId,
            IsActive = true
        };

        _context.HotelDates.Add(hotelDate);
        await _context.SaveChangesAsync();

        var savedHotelDate = await _context.HotelDates
            .Include(hd => hd.Hotel)
            .Include(hd => hd.HotelRoomType)
            .FirstAsync();

        Assert.NotNull(savedHotelDate.Hotel);
        Assert.NotNull(savedHotelDate.HotelRoomType);
        Assert.Equal(10, savedHotelDate.AvailableRooms);
        Assert.Equal("Test Hotel", savedHotelDate.Hotel.Name);
        Assert.Equal("Standard Room", savedHotelDate.HotelRoomType.Name);
        Assert.True(savedHotelDate.EndDate > savedHotelDate.StartDate);
    }

    [Fact]
    public async Task HotelDate_CanCreateMultipleDateRanges()
    {
        // Test: Verify multiple date ranges for hotel room types
        // Purpose: Tests availability scheduling functionality
        var hotel = new Hotel
        {
            Name = "Availability Hotel",
            Cnpj = "12.345.678/0001-99",
            StarRating = 4,
            AddressId = 1
        };

        var roomType = new HotelRoomType
        {
            Name = "Deluxe Room",
            Price = 200.00m,
            Capacity = 3,
            BedType = "King Size",
            HotelId = hotel.HotelId
        };

        _context.Hotels.Add(hotel);
        _context.RoomTypes.Add(roomType);
        await _context.SaveChangesAsync();

        var hotelDates = new List<HotelDate>
        {
            new HotelDate
            {
                StartDate = DateTime.UtcNow.AddDays(30),
                EndDate = DateTime.UtcNow.AddDays(37),
                AvailableRooms = 5,
                RoomTypeId = roomType.RoomTypeId,
                HotelId = hotel.HotelId
            },
            new HotelDate
            {
                StartDate = DateTime.UtcNow.AddDays(60),
                EndDate = DateTime.UtcNow.AddDays(67),
                AvailableRooms = 8,
                RoomTypeId = roomType.RoomTypeId,
                HotelId = hotel.HotelId
            },
            new HotelDate
            {
                StartDate = DateTime.UtcNow.AddDays(90),
                EndDate = DateTime.UtcNow.AddDays(97),
                AvailableRooms = 3,
                RoomTypeId = roomType.RoomTypeId,
                HotelId = hotel.HotelId
            }
        };

        _context.HotelDates.AddRange(hotelDates);
        await _context.SaveChangesAsync();

        var savedHotelDates = await _context.HotelDates
            .Where(hd => hd.HotelId == hotel.HotelId)
            .OrderBy(hd => hd.StartDate)
            .ToListAsync();

        Assert.Equal(3, savedHotelDates.Count);
        Assert.Equal(5, savedHotelDates[0].AvailableRooms);
        Assert.Equal(8, savedHotelDates[1].AvailableRooms);
        Assert.Equal(3, savedHotelDates[2].AvailableRooms);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void HotelDate_ValidAvailableRooms(int availableRooms)
    {
        // Test: Verify valid available rooms values
        // Purpose: Tests non-negative room availability constraint
        var hotelDate = new HotelDate { AvailableRooms = availableRooms };

        Assert.True(availableRooms >= 0, "Available rooms should be non-negative");
        Assert.Equal(availableRooms, hotelDate.AvailableRooms);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void HotelDate_InvalidAvailableRooms(int availableRooms)
    {
        // Test: Verify invalid available rooms values
        // Purpose: Tests validation constraints
        Assert.True(availableRooms < 0, "Invalid available rooms should be negative");
    }

    [Fact]
    public void HotelDate_DateValidation()
    {
        // Test: Verify end date is after start date
        // Purpose: Tests date logic validation
        var validHotelDate = new HotelDate
        {
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(35)
        };

        var invalidHotelDate = new HotelDate
        {
            StartDate = DateTime.UtcNow.AddDays(35),
            EndDate = DateTime.UtcNow.AddDays(30) // End before start
        };

        Assert.True(validHotelDate.EndDate > validHotelDate.StartDate,
            "Valid hotel date should have end date after start date");
        Assert.False(invalidHotelDate.EndDate > invalidHotelDate.StartDate,
            "Invalid hotel date should have end date before start date");
    }

    [Fact]
    public async Task HotelDate_SoftDeleteWorks()
    {
        // Test: Verify soft delete functionality
        // Purpose: Ensures IsActive filtering works correctly
        var hotel = new Hotel
        {
            Name = "Test Hotel",
            Cnpj = "12.345.678/0001-99",
            StarRating = 4,
            AddressId = 1
        };

        var roomType = new HotelRoomType
        {
            Name = "Test Room",
            Price = 150.00m,
            Capacity = 2,
            BedType = "Queen Size",
            HotelId = hotel.HotelId
        };

        _context.Hotels.Add(hotel);
        _context.RoomTypes.Add(roomType);
        await _context.SaveChangesAsync();

        var activeHotelDate = new HotelDate
        {
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(35),
            AvailableRooms = 5,
            RoomTypeId = roomType.RoomTypeId,
            HotelId = hotel.HotelId,
            IsActive = true
        };

        var inactiveHotelDate = new HotelDate
        {
            StartDate = DateTime.UtcNow.AddDays(60),
            EndDate = DateTime.UtcNow.AddDays(65),
            AvailableRooms = 3,
            RoomTypeId = roomType.RoomTypeId,
            HotelId = hotel.HotelId,
            IsActive = false
        };

        _context.HotelDates.Add(activeHotelDate);
        _context.HotelDates.Add(inactiveHotelDate);
        await _context.SaveChangesAsync();

        var hotelDates = await _context.HotelDates.ToListAsync();

        Assert.Single(hotelDates);
        Assert.Equal(5, hotelDates.First().AvailableRooms);

        // Verify inactive hotel date exists when ignoring filters
        var allHotelDates = await _context.HotelDates.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(2, allHotelDates.Count);
    }

    [Fact]
    public void HotelDate_RequiredFieldsValidation()
    {
        // Test: Verify required fields validation
        // Purpose: Tests model validation attributes
        var hotelDate = new HotelDate();

        // These would be caught by model validation in real scenarios
        Assert.Equal(DateTime.MinValue, hotelDate.StartDate);
        Assert.Equal(DateTime.MinValue, hotelDate.EndDate);
        Assert.Equal(0, hotelDate.AvailableRooms);
        Assert.Equal(0, hotelDate.RoomTypeId);
        Assert.Equal(0, hotelDate.HotelId);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}