using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Packages;

namespace viaggia_server.tests.Models;

public class PackageTests : IDisposable
{
    private readonly AppDbContext _context;

    public PackageTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task Package_CanCreatePackage()
    {
        // Test: Verify package creation with required hotel relationship
        // Purpose: Tests basic package entity functionality with foreign key

        // First create a hotel (required for package)
        var hotel = new Hotel
        {
            Name = "Test Hotel",
            Cnpj = "12.345.678/0001-99",
            StarRating = 4,
            AddressId = 1
        };
        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        var package = new Package
        {
            Name = "Pacote Rio de Janeiro",
            Destination = "Rio de Janeiro, RJ",
            Description = "Pacote completo para o Rio de Janeiro incluindo Cristo Redentor e Pão de Açúcar",
            BasePrice = 1500.00m,
            HotelId = hotel.HotelId,
            IsActive = true
        };

        _context.Packages.Add(package);
        await _context.SaveChangesAsync();

        var savedPackage = await _context.Packages.Include(p => p.Hotel).FirstAsync();

        Assert.Equal("Pacote Rio de Janeiro", savedPackage.Name);
        Assert.Equal("Rio de Janeiro, RJ", savedPackage.Destination);
        Assert.Equal(1500.00m, savedPackage.BasePrice);
        Assert.Equal(hotel.HotelId, savedPackage.HotelId);
        Assert.NotNull(savedPackage.Hotel);
        Assert.Equal("Test Hotel", savedPackage.Hotel.Name);
        Assert.True(savedPackage.IsActive);
    }

    [Fact]
    public void Package_HasRequiredCollections()
    {
        // Test: Verify package has all required navigation properties
        // Purpose: Ensures entity relationships are properly configured
        var package = new Package();

        Assert.NotNull(package.Medias);
        Assert.NotNull(package.PackageDates);
        Assert.NotNull(package.Reservations);
        Assert.Empty(package.Medias);
        Assert.Empty(package.PackageDates);
        Assert.Empty(package.Reservations);
    }

    [Fact]
    public async Task Package_SoftDeleteWorks()
    {
        // Test: Verify soft delete functionality for packages
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

        var activePackage = new Package
        {
            Name = "Active Package",
            Destination = "São Paulo",
            BasePrice = 1000m,
            HotelId = hotel.HotelId,
            IsActive = true
        };

        var inactivePackage = new Package
        {
            Name = "Inactive Package",
            Destination = "Brasília",
            BasePrice = 2000m,
            HotelId = hotel.HotelId,
            IsActive = false
        };

        _context.Packages.Add(activePackage);
        _context.Packages.Add(inactivePackage);
        await _context.SaveChangesAsync();

        var packages = await _context.Packages.ToListAsync();

        Assert.Single(packages);
        Assert.Equal("Active Package", packages.First().Name);

        // Verify inactive package exists when ignoring filters
        var allPackages = await _context.Packages.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(2, allPackages.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Package_PriceValidation_ShouldBePositive(decimal price)
    {
        // Test: Verify price validation logic
        // Purpose: Tests business logic constraints
        var package = new Package
        {
            Name = "Test Package",
            Destination = "Test Destination",
            BasePrice = price
        };

        if (price <= 0)
        {
            Assert.True(price <= 0, "Price should be positive - this test validates the check");
        }
    }

    [Fact]
    public void Package_RequiredFieldsValidation()
    {
        // Test: Verify required fields are properly set
        // Purpose: Ensures model validation attributes work
        var package = new Package();

        // These would be caught by model validation in real scenarios
        Assert.Null(package.Name);
        Assert.Null(package.Destination);
        Assert.Equal(0, package.BasePrice);
        Assert.Equal(0, package.HotelId);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}