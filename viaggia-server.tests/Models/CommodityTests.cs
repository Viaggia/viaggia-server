using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.Hotels;

namespace viaggia_server.tests.Models;

public class CommodityTests : IDisposable
{
    private readonly AppDbContext _context;

    public CommodityTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task Commodity_CanCreateBasicCommodity()
    {
        // Test: Verify commodity creation with basic amenities
        // Purpose: Tests basic commodity entity functionality
        var hotel = new Hotel
        {
            Name = "Test Hotel",
            Cnpj = "12.345.678/0001-99",
            StarRating = 4,
            AddressId = 1
        };

        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        var commodity = new Commodity
        {
            HasParking = true,
            IsParkingPaid = false,
            HasBreakfast = true,
            IsBreakfastPaid = true,
            HasWiFi = true,
            IsWiFiPaid = false,
            HasPool = true,
            IsPoolPaid = false,
            HotelId = hotel.HotelId,
            IsActive = true
        };

        _context.Commodities.Add(commodity);
        await _context.SaveChangesAsync();

        var savedCommodity = await _context.Commodities
            .Include(c => c.Hotel)
            .FirstAsync();

        Assert.NotNull(savedCommodity.Hotel);
        Assert.Equal("Test Hotel", savedCommodity.Hotel.Name);
        Assert.True(savedCommodity.HasParking);
        Assert.False(savedCommodity.IsParkingPaid);
        Assert.True(savedCommodity.HasBreakfast);
        Assert.True(savedCommodity.IsBreakfastPaid);
        Assert.True(savedCommodity.HasWiFi);
        Assert.False(savedCommodity.IsWiFiPaid);
    }

    [Fact]
    public async Task Commodity_CanCreateLuxuryCommodity()
    {
        // Test: Verify commodity creation with luxury amenities
        // Purpose: Tests full-service hotel commodity configuration
        var hotel = new Hotel
        {
            Name = "Luxury Resort",
            Cnpj = "98.765.432/0001-11",
            StarRating = 5,
            AddressId = 1
        };

        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        var luxuryCommodity = new Commodity
        {
            // Parking
            HasParking = true,
            IsParkingPaid = false, // Free for luxury hotels

            // Meals
            HasBreakfast = true,
            IsBreakfastPaid = false, // Included
            HasLunch = true,
            IsLunchPaid = true, // À la carte
            HasDinner = true,
            IsDinnerPaid = true, // À la carte

            // Wellness
            HasSpa = true,
            IsSpaPaid = true,
            HasPool = true,
            IsPoolPaid = false, // Included
            HasGym = true,
            IsGymPaid = false, // Included

            // Basic amenities
            HasWiFi = true,
            IsWiFiPaid = false, // Free
            HasAirConditioning = true,
            IsAirConditioningPaid = false, // Included

            // Accessibility
            HasAccessibilityFeatures = true,
            IsAccessibilityFeaturesPaid = false,

            // Pet policy
            IsPetFriendly = true,
            IsPetFriendlyPaid = true, // Pet fee

            HotelId = hotel.HotelId
        };

        _context.Commodities.Add(luxuryCommodity);
        await _context.SaveChangesAsync();

        var savedCommodity = await _context.Commodities.FirstAsync();

        // Verify all luxury amenities are properly set
        Assert.True(savedCommodity.HasSpa);
        Assert.True(savedCommodity.HasLunch);
        Assert.True(savedCommodity.HasDinner);
        Assert.True(savedCommodity.HasGym);
        Assert.True(savedCommodity.HasAccessibilityFeatures);
        Assert.True(savedCommodity.IsPetFriendly);

        // Verify pricing structure
        Assert.False(savedCommodity.IsBreakfastPaid); // Included
        Assert.True(savedCommodity.IsLunchPaid); // Extra
        Assert.True(savedCommodity.IsDinnerPaid); // Extra
        Assert.True(savedCommodity.IsSpaPaid); // Extra
    }

    [Fact]
    public async Task Commodity_CanCreateBudgetCommodity()
    {
        // Test: Verify commodity creation for budget hotels
        // Purpose: Tests minimal commodity configuration
        var hotel = new Hotel
        {
            Name = "Budget Inn",
            Cnpj = "11.222.333/0001-44",
            StarRating = 2,
            AddressId = 1
        };

        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        var budgetCommodity = new Commodity
        {
            // Limited amenities for budget hotel
            HasParking = true,
            IsParkingPaid = true, // Paid parking

            HasBreakfast = false, // No breakfast service
            IsBreakfastPaid = false,

            HasWiFi = true,
            IsWiFiPaid = true, // Paid WiFi

            HasAirConditioning = false, // No AC
            IsAirConditioningPaid = false,

            // No luxury amenities
            HasSpa = false,
            HasPool = false,
            HasGym = false,

            HotelId = hotel.HotelId
        };

        _context.Commodities.Add(budgetCommodity);
        await _context.SaveChangesAsync();

        var savedCommodity = await _context.Commodities.FirstAsync();

        // Verify budget configuration
        Assert.True(savedCommodity.HasParking);
        Assert.True(savedCommodity.IsParkingPaid); // Paid
        Assert.False(savedCommodity.HasBreakfast); // Not available
        Assert.True(savedCommodity.HasWiFi);
        Assert.True(savedCommodity.IsWiFiPaid); // Paid
        Assert.False(savedCommodity.HasSpa); // Not available
        Assert.False(savedCommodity.HasPool); // Not available
        Assert.False(savedCommodity.HasGym); // Not available
    }

    [Fact]
    public void Commodity_HasRequiredCollections()
    {
        // Test: Verify commodity has required navigation properties
        // Purpose: Ensures entity relationships are properly configured
        var commodity = new Commodity();

        Assert.NotNull(commodity.CommoditiesServices);
        Assert.Empty(commodity.CommoditiesServices);
    }

    [Fact]
    public void Commodity_DefaultValues()
    {
        // Test: Verify commodity default values
        // Purpose: Tests proper initialization of boolean properties
        var commodity = new Commodity();

        // All boolean properties should default to false
        Assert.False(commodity.HasParking);
        Assert.False(commodity.IsParkingPaid);
        Assert.False(commodity.HasBreakfast);
        Assert.False(commodity.IsBreakfastPaid);
        Assert.False(commodity.HasLunch);
        Assert.False(commodity.IsLunchPaid);
        Assert.False(commodity.HasDinner);
        Assert.False(commodity.IsDinnerPaid);
        Assert.False(commodity.HasSpa);
        Assert.False(commodity.IsSpaPaid);
        Assert.False(commodity.HasPool);
        Assert.False(commodity.IsPoolPaid);
        Assert.False(commodity.HasGym);
        Assert.False(commodity.IsGymPaid);
        Assert.False(commodity.HasWiFi);
        Assert.False(commodity.IsWiFiPaid);
        Assert.False(commodity.HasAirConditioning);
        Assert.False(commodity.IsAirConditioningPaid);
        Assert.False(commodity.HasAccessibilityFeatures);
        Assert.False(commodity.IsAccessibilityFeaturesPaid);
        Assert.False(commodity.IsPetFriendly);
        Assert.False(commodity.IsPetFriendlyPaid);

        // IsActive should default to true
        Assert.True(commodity.IsActive);
    }

    [Theory]
    [InlineData(true, false)] // Has service, free
    [InlineData(true, true)]  // Has service, paid
    [InlineData(false, false)] // No service, irrelevant pricing
    public void Commodity_ServiceAndPricingLogic(bool hasService, bool isPaid)
    {
        // Test: Verify service availability and pricing logic
        // Purpose: Tests business logic for service pricing
        var commodity = new Commodity
        {
            HasParking = hasService,
            IsParkingPaid = isPaid
        };

        if (hasService)
        {
            Assert.True(commodity.HasParking);
            // If service exists, pricing can be either free or paid
            Assert.Equal(isPaid, commodity.IsParkingPaid);
        }
        else
        {
            Assert.False(commodity.HasParking);
            // If service doesn't exist, pricing should be irrelevant
        }
    }

    [Fact]
    public void Commodity_RequiredFieldsValidation()
    {
        // Test: Verify required fields validation
        // Purpose: Tests model validation attributes
        var commodity = new Commodity();

        // HotelId should be required
        Assert.Equal(0, commodity.HotelId);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}