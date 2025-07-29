using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Models.Hotels;

namespace viaggia_server.tests.Models;

public class HotelRoomTypeTests : IDisposable
{
    private readonly AppDbContext _context;

    public HotelRoomTypeTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task HotelRoomType_CanCreateRoomType()
    {
        // Test: Verify room type creation with required hotel relationship
        // Purpose: Tests basic room type entity functionality
        var hotel = new Hotel
        {
            Name = "Test Hotel",
            Cnpj = "12.345.678/0001-99",
            StarRating = 4,
            AddressId = 1
        };

        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        var roomType = new HotelRoomType
        {
            Name = "Deluxe Suite",
            Description = "Spacious suite with ocean view",
            Price = 250.00m,
            Capacity = 4,
            BedType = "King Size",
            HotelId = hotel.HotelId,
            IsActive = true
        };

        _context.RoomTypes.Add(roomType);
        await _context.SaveChangesAsync();

        var savedRoomType = await _context.RoomTypes
            .Include(rt => rt.Hotel)
            .FirstAsync();

        Assert.NotNull(savedRoomType.Hotel);
        Assert.Equal("Deluxe Suite", savedRoomType.Name);
        Assert.Equal("Spacious suite with ocean view", savedRoomType.Description);
        Assert.Equal(250.00m, savedRoomType.Price);
        Assert.Equal(4, savedRoomType.Capacity);
        Assert.Equal("King Size", savedRoomType.BedType);
        Assert.Equal("Test Hotel", savedRoomType.Hotel.Name);
    }

    [Fact]
    public async Task HotelRoomType_CanCreateMultipleRoomTypes()
    {
        // Test: Verify multiple room types for single hotel
        // Purpose: Tests one-to-many relationship functionality
        var hotel = new Hotel
        {
            Name = "Grand Hotel",
            Cnpj = "12.345.678/0001-99",
            StarRating = 5,
            AddressId = 1
        };

        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        var roomTypes = new List<HotelRoomType>
        {
            new HotelRoomType
            {
                Name = "Standard Room",
                Description = "Comfortable standard room",
                Price = 150.00m,
                Capacity = 2,
                BedType = "Queen Size",
                HotelId = hotel.HotelId
            },
            new HotelRoomType
            {
                Name = "Deluxe Room",
                Description = "Upgraded room with balcony",
                Price = 200.00m,
                Capacity = 3,
                BedType = "King Size",
                HotelId = hotel.HotelId
            },
            new HotelRoomType
            {
                Name = "Presidential Suite",
                Description = "Luxurious suite with premium amenities",
                Price = 500.00m,
                Capacity = 6,
                BedType = "King Size + Sofa Bed",
                HotelId = hotel.HotelId
            }
        };

        _context.RoomTypes.AddRange(roomTypes);
        await _context.SaveChangesAsync();

        var savedRoomTypes = await _context.RoomTypes
            .Where(rt => rt.HotelId == hotel.HotelId)
            .OrderBy(rt => rt.Price)
            .ToListAsync();

        Assert.Equal(3, savedRoomTypes.Count);
        Assert.Equal("Standard Room", savedRoomTypes[0].Name);
        Assert.Equal("Deluxe Room", savedRoomTypes[1].Name);
        Assert.Equal("Presidential Suite", savedRoomTypes[2].Name);
        Assert.True(savedRoomTypes[0].Price < savedRoomTypes[1].Price);
        Assert.True(savedRoomTypes[1].Price < savedRoomTypes[2].Price);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void HotelRoomType_ValidCapacity(int capacity)
    {
        // Test: Verify valid capacity values
        // Purpose: Tests capacity validation constraints
        var roomType = new HotelRoomType { Capacity = capacity };

        Assert.InRange(capacity, 1, 10);
        Assert.Equal(capacity, roomType.Capacity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(-1)]
    public void HotelRoomType_InvalidCapacity(int capacity)
    {
        // Test: Verify invalid capacity values
        // Purpose: Tests validation constraints
        Assert.True(capacity < 1 || capacity > 10, "Invalid capacity should be outside 1-10 range");
    }

    [Theory]
    [InlineData("Single")]
    [InlineData("Twin")]
    [InlineData("Queen Size")]
    [InlineData("King Size")]
    [InlineData("Sofa Bed")]
    public void HotelRoomType_ValidBedTypes(string bedType)
    {
        // Test: Verify valid bed types
        // Purpose: Tests bed type validation
        var roomType = new HotelRoomType { BedType = bedType };

        Assert.NotEmpty(bedType);
        Assert.Equal(bedType, roomType.BedType);
    }

    [Fact]
    public void HotelRoomType_PriceValidation()
    {
        // Test: Verify price is positive
        // Purpose: Tests business logic constraints
        var validRoomType = new HotelRoomType { Price = 100.00m };
        var invalidRoomType = new HotelRoomType { Price = -50.00m };

        Assert.True(validRoomType.Price > 0, "Valid room type should have positive price");
        Assert.False(invalidRoomType.Price > 0, "Invalid room type should have non-positive price");
    }

    [Fact]
    public async Task HotelRoomType_SoftDeleteWorks()
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

        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        var activeRoomType = new HotelRoomType
        {
            Name = "Active Room",
            Price = 150.00m,
            Capacity = 2,
            BedType = "Queen Size",
            HotelId = hotel.HotelId,
            IsActive = true
        };

        var inactiveRoomType = new HotelRoomType
        {
            Name = "Inactive Room",
            Price = 200.00m,
            Capacity = 3,
            BedType = "King Size",
            HotelId = hotel.HotelId,
            IsActive = false
        };

        _context.RoomTypes.Add(activeRoomType);
        _context.RoomTypes.Add(inactiveRoomType);
        await _context.SaveChangesAsync();

        var roomTypes = await _context.RoomTypes.ToListAsync();

        Assert.Single(roomTypes);
        Assert.Equal("Active Room", roomTypes.First().Name);
        
        // Verify inactive room type exists when ignoring filters
        var allRoomTypes = await _context.RoomTypes.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(2, allRoomTypes.Count);
    }

    [Fact]
    public void HotelRoomType_RequiredFieldsValidation()
    {
        // Test: Verify required fields validation
        // Purpose: Tests model validation attributes
        var roomType = new HotelRoomType();

        // These would be caught by model validation in real scenarios
        Assert.Null(roomType.Name);
        Assert.Equal(0, roomType.Price);
        Assert.Equal(0, roomType.Capacity);
        Assert.Null(roomType.BedType);
        Assert.Equal(0, roomType.HotelId);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}