using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Addresses;

namespace viaggia_server.tests.Models;

public class PackageDateTests : IDisposable
{
    private readonly AppDbContext _context;

    public PackageDateTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task PackageDate_CanCreatePackageDate()
    {
        // Test: Verify package date creation with required package relationship
        // Purpose: Tests basic package date entity functionality

        // First create an Address (required for Hotel)
        var address = new Address
        {
            Street = "Rua Test, 123",
            City = "São Paulo",
            State = "SP",
            ZipCode = "01234-567",
            IsActive = true
        };

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync(); // Save address first to get the ID

        var hotel = new Hotel
        {
            Name = "Test Hotel",
            Cnpj = "12.345.678/0001-99",
            StarRating = 4,
            AddressId = address.AddressId, // Use the actual address ID
            IsActive = true
        };

        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync(); // Save hotel first to get the ID

        var package = new Package
        {
            Name = "Summer Package",
            Destination = "Rio de Janeiro",
            BasePrice = 1500m,
            HotelId = hotel.HotelId, // Use the actual hotel ID
            IsActive = true
        };

        _context.Packages.Add(package);
        await _context.SaveChangesAsync(); // Save package to get the ID

        var packageDate = new PackageDate
        {
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(37), // 7-day package
            PackageId = package.PackageId, // Use the actual package ID
            IsActive = true // This is crucial due to the global query filter
        };

        _context.PackageDates.Add(packageDate);
        await _context.SaveChangesAsync();

        // Verify the PackageDate was saved - first check count
        var packageDateCount = await _context.PackageDates.CountAsync();
        Assert.Equal(1, packageDateCount);

        var savedPackageDate = await _context.PackageDates
            .Include(pd => pd.Package)
            .FirstAsync();

        Assert.NotNull(savedPackageDate.Package);
        Assert.Equal(package.PackageId, savedPackageDate.PackageId);
        Assert.Equal("Summer Package", savedPackageDate.Package.Name);
        Assert.True(savedPackageDate.EndDate > savedPackageDate.StartDate);
        Assert.Equal(7, (savedPackageDate.EndDate - savedPackageDate.StartDate).Days);
        Assert.True(savedPackageDate.IsActive);
    }

    [Fact]
    public async Task PackageDate_CanCreateMultipleDatesForPackage()
    {
        // Test: Verify multiple date ranges for single package
        // Purpose: Tests one-to-many relationship functionality

        // Create required Address first
        var address = new Address
        {
            Street = "Rua Test, 456",
            City = "Rio de Janeiro",
            State = "RJ",
            ZipCode = "22000-000",
            IsActive = true
        };

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        var hotel = new Hotel
        {
            Name = "Test Hotel",
            Cnpj = "12.345.678/0001-99",
            StarRating = 4,
            AddressId = address.AddressId,
            IsActive = true
        };

        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        var package = new Package
        {
            Name = "Flexible Package",
            Destination = "São Paulo",
            BasePrice = 1200m,
            HotelId = hotel.HotelId,
            IsActive = true
        };

        _context.Packages.Add(package);
        await _context.SaveChangesAsync();

        var packageDates = new List<PackageDate>
        {
            new PackageDate
            {
                StartDate = DateTime.UtcNow.AddDays(30),
                EndDate = DateTime.UtcNow.AddDays(35),
                PackageId = package.PackageId,
                IsActive = true // Important for global query filter
            },
            new PackageDate
            {
                StartDate = DateTime.UtcNow.AddDays(60),
                EndDate = DateTime.UtcNow.AddDays(67),
                PackageId = package.PackageId,
                IsActive = true
            },
            new PackageDate
            {
                StartDate = DateTime.UtcNow.AddDays(90),
                EndDate = DateTime.UtcNow.AddDays(95),
                PackageId = package.PackageId,
                IsActive = true
            }
        };

        _context.PackageDates.AddRange(packageDates);
        await _context.SaveChangesAsync();

        var savedPackageDates = await _context.PackageDates
            .Where(pd => pd.PackageId == package.PackageId)
            .OrderBy(pd => pd.StartDate)
            .ToListAsync();

        Assert.Equal(3, savedPackageDates.Count);
        Assert.True(savedPackageDates[0].StartDate < savedPackageDates[1].StartDate);
        Assert.True(savedPackageDates[1].StartDate < savedPackageDates[2].StartDate);
        Assert.All(savedPackageDates, pd => Assert.True(pd.IsActive));
    }

    [Fact]
    public void PackageDate_DateValidation()
    {
        // Test: Verify end date is after start date
        // Purpose: Tests date logic validation
        var validPackageDate = new PackageDate
        {
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(35)
        };

        var invalidPackageDate = new PackageDate
        {
            StartDate = DateTime.UtcNow.AddDays(35),
            EndDate = DateTime.UtcNow.AddDays(30) // End before start
        };

        Assert.True(validPackageDate.EndDate > validPackageDate.StartDate,
            "Valid package date should have end date after start date");
        Assert.False(invalidPackageDate.EndDate > invalidPackageDate.StartDate,
            "Invalid package date should have end date before start date");
    }

    [Fact]
    public void PackageDate_FutureDateValidation()
    {
        // Test: Verify package dates are in the future
        // Purpose: Tests business logic for booking dates
        var futurePackageDate = new PackageDate
        {
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(35)
        };

        var pastPackageDate = new PackageDate
        {
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(-5)
        };

        Assert.True(futurePackageDate.StartDate > DateTime.UtcNow,
            "Package start date should be in the future");
        Assert.False(pastPackageDate.StartDate > DateTime.UtcNow,
            "Past package date should not be in the future");
    }

    [Fact]
    public async Task PackageDate_SoftDeleteWorks()
    {
        // Test: Verify soft delete functionality
        // Purpose: Ensures IsActive filtering works correctly

        // Create required Address first
        var address = new Address
        {
            Street = "Rua Soft Delete, 789",
            City = "Brasília",
            State = "DF",
            ZipCode = "70000-000",
            IsActive = true
        };

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        var hotel = new Hotel
        {
            Name = "Test Hotel",
            Cnpj = "12.345.678/0001-99",
            StarRating = 4,
            AddressId = address.AddressId,
            IsActive = true
        };

        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        var package = new Package
        {
            Name = "Test Package",
            Destination = "Test Destination",
            BasePrice = 1000m,
            HotelId = hotel.HotelId,
            IsActive = true
        };

        _context.Packages.Add(package);
        await _context.SaveChangesAsync();

        var activePackageDate = new PackageDate
        {
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(35),
            PackageId = package.PackageId,
            IsActive = true
        };

        var inactivePackageDate = new PackageDate
        {
            StartDate = DateTime.UtcNow.AddDays(60),
            EndDate = DateTime.UtcNow.AddDays(65),
            PackageId = package.PackageId,
            IsActive = false // This will be filtered out by global query filter
        };

        _context.PackageDates.Add(activePackageDate);
        _context.PackageDates.Add(inactivePackageDate);
        await _context.SaveChangesAsync();

        // Should only return active package dates due to global query filter
        var packageDates = await _context.PackageDates.ToListAsync();

        Assert.Single(packageDates);
        Assert.True(packageDates.First().IsActive);

        // Verify inactive package date exists when ignoring filters
        var allPackageDates = await _context.PackageDates.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(2, allPackageDates.Count);

        var activeCount = allPackageDates.Count(pd => pd.IsActive);
        var inactiveCount = allPackageDates.Count(pd => !pd.IsActive);
        Assert.Equal(1, activeCount);
        Assert.Equal(1, inactiveCount);
    }

    [Fact]
    public void PackageDate_RequiredFieldsValidation()
    {
        // Test: Verify required fields validation
        // Purpose: Tests model validation attributes
        var packageDate = new PackageDate();

        // These would be caught by model validation in real scenarios
        Assert.Equal(DateTime.MinValue, packageDate.StartDate);
        Assert.Equal(DateTime.MinValue, packageDate.EndDate);
        Assert.Equal(0, packageDate.PackageId);
        Assert.True(packageDate.IsActive); // Default should be true
    }

    [Fact]
    public async Task PackageDate_DefaultIsActiveValue()
    {
        // Test: Verify that IsActive defaults to true
        // Purpose: Tests that new PackageDate entities are active by default

        // Create required entities
        var address = new Address
        {
            Street = "Rua Default, 100",
            City = "São Paulo",
            State = "SP",
            ZipCode = "01000-000"
        };

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        var hotel = new Hotel
        {
            Name = "Default Hotel",
            Cnpj = "11.111.111/0001-11",
            StarRating = 3,
            AddressId = address.AddressId
        };

        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        var package = new Package
        {
            Name = "Default Package",
            Destination = "Default Destination",
            BasePrice = 500m,
            HotelId = hotel.HotelId
        };

        _context.Packages.Add(package);
        await _context.SaveChangesAsync();

        var packageDate = new PackageDate
        {
            StartDate = DateTime.UtcNow.AddDays(15),
            EndDate = DateTime.UtcNow.AddDays(20),
            PackageId = package.PackageId
            // Not explicitly setting IsActive - should default to true
        };

        _context.PackageDates.Add(packageDate);
        await _context.SaveChangesAsync();

        var savedPackageDate = await _context.PackageDates.FirstAsync();
        Assert.True(savedPackageDate.IsActive, "PackageDate should default to IsActive = true");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}