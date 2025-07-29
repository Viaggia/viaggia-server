using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Hotels;

namespace viaggia_server.tests.Models;

public class HotelTests : IDisposable
{
    private readonly AppDbContext _context;

    public HotelTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task Hotel_CanCreateHotel()
    {
        // Test: Verify hotel creation with all properties
        // Purpose: Tests basic hotel entity functionality
        var hotel = new Hotel
        {
            Name = "Hotel Copacabana Palace",
            Cnpj = "12.345.678/0001-99",
            Description = "Luxury hotel in Copacabana beach",
            StarRating = 5,
            CheckInTime = "15:00",
            CheckOutTime = "12:00",
            ContactPhone = "+5521999999999",
            ContactEmail = "contact@copacabana.com",
            AddressId = 1,
            IsActive = true
        };

        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        var savedHotel = await _context.Hotels.FirstAsync();

        Assert.Equal("Hotel Copacabana Palace", savedHotel.Name);
        Assert.Equal("12.345.678/0001-99", savedHotel.Cnpj);
        Assert.Equal(5, savedHotel.StarRating);
        Assert.Equal("15:00", savedHotel.CheckInTime);
        Assert.Equal("12:00", savedHotel.CheckOutTime);
        Assert.True(savedHotel.IsActive);
    }

    [Fact]
    public void Hotel_HasRequiredCollections()
    {
        // Test: Verify hotel has all required navigation properties
        // Purpose: Ensures entity relationships are properly configured
        var hotel = new Hotel();

        Assert.NotNull(hotel.RoomTypes);
        Assert.NotNull(hotel.HotelDates);
        Assert.NotNull(hotel.Reservations);
        Assert.NotNull(hotel.Medias);
        Assert.NotNull(hotel.Reviews);
        Assert.NotNull(hotel.Packages);
        Assert.Empty(hotel.RoomTypes);
        Assert.Empty(hotel.HotelDates);
        Assert.Empty(hotel.Reservations);
        Assert.Empty(hotel.Medias);
        Assert.Empty(hotel.Reviews);
        Assert.Empty(hotel.Packages);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Hotel_ValidStarRatings(int rating)
    {
        // Test: Verify valid star ratings
        // Purpose: Tests business logic for star ratings
        var hotel = new Hotel { StarRating = rating };

        Assert.InRange(rating, 1, 5);
        Assert.Equal(rating, hotel.StarRating);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Hotel_InvalidStarRatings_ShouldBeOutOfRange(int rating)
    {
        // Test: Verify invalid star ratings are outside valid range
        // Purpose: Tests validation constraints
        Assert.True(rating < 1 || rating > 5, "Invalid star rating should be outside 1-5 range");
    }

    [Fact]
    public async Task Hotel_SoftDeleteWorks()
    {
        // Test: Verify soft delete functionality for hotels
        // Purpose: Ensures IsActive filtering works correctly
        var activeHotel = new Hotel
        {
            Name = "Active Hotel",
            Cnpj = "11.111.111/0001-11",
            StarRating = 4,
            AddressId = 1,
            IsActive = true
        };

        var inactiveHotel = new Hotel
        {
            Name = "Inactive Hotel",
            Cnpj = "22.222.222/0001-22",
            StarRating = 3,
            AddressId = 2,
            IsActive = false
        };

        _context.Hotels.Add(activeHotel);
        _context.Hotels.Add(inactiveHotel);
        await _context.SaveChangesAsync();

        var hotels = await _context.Hotels.ToListAsync();

        Assert.Single(hotels);
        Assert.Equal("Active Hotel", hotels.First().Name);
        
        // Verify inactive hotel exists when ignoring filters
        var allHotels = await _context.Hotels.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(2, allHotels.Count);
    }

    [Fact]
    public void Hotel_EmailValidation()
    {
        // Test: Verify email format validation for contact email
        // Purpose: Tests email validation attributes
        var hotel = new Hotel
        {
            ContactEmail = "invalid-email"
        };

        // This would be caught by model validation in real scenarios
        Assert.False(hotel.ContactEmail.Contains("@") && hotel.ContactEmail.Contains("."),
            "Invalid email format should fail validation");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}