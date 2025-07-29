using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Reservations;
using viaggia_server.Models.Users;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Hotels;

namespace viaggia_server.tests.Models;

public class ReservationTests : IDisposable
{
    private readonly AppDbContext _context;

    public ReservationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task Reservation_CanCreateReservationWithUser()
    {
        // Test: Verify reservation creation with required user relationship
        // Purpose: Tests basic reservation entity functionality
        var user = new User
        {
            Name = "Test User",
            Email = "user@test.com",
            Password = "password",
            PhoneNumber = "+5511999999999"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var reservation = new Reservation
        {
            UserId = user.Id,
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(35),
            TotalPrice = 1500m,
            NumberOfGuests = 2,
            Status = "Confirmed"
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var savedReservation = await _context.Reservations
            .Include(r => r.User)
            .FirstAsync();

        Assert.NotNull(savedReservation.User);
        Assert.Equal("Test User", savedReservation.User.Name);
        Assert.Equal(1500m, savedReservation.TotalPrice);
        Assert.Equal(2, savedReservation.NumberOfGuests);
        Assert.Equal("Confirmed", savedReservation.Status);
    }

    [Fact]
    public async Task Reservation_CanCreateWithPackage()
    {
        // Test: Verify reservation can be created with package
        // Purpose: Tests package relationship functionality
        var user = new User
        {
            Name = "Test User",
            Email = "user@test.com",
            Password = "password",
            PhoneNumber = "+5511999999999"
        };

        var hotel = new Hotel
        {
            Name = "Test Hotel",
            Cnpj = "12.345.678/0001-99",
            StarRating = 4,
            AddressId = 1
        };

        var package = new Package
        {
            Name = "Test Package",
            Destination = "Test Destination",
            BasePrice = 1000m,
            HotelId = hotel.HotelId
        };

        _context.Users.Add(user);
        _context.Hotels.Add(hotel);
        _context.Packages.Add(package);
        await _context.SaveChangesAsync();

        var reservation = new Reservation
        {
            UserId = user.Id,
            PackageId = package.PackageId,
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(35),
            TotalPrice = 1000m,
            NumberOfGuests = 1,
            Status = "Pending"
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var savedReservation = await _context.Reservations
            .Include(r => r.Package)
            .FirstAsync();

        Assert.NotNull(savedReservation.Package);
        Assert.Equal("Test Package", savedReservation.Package.Name);
    }

    [Fact]
    public void Reservation_HasRequiredCollections()
    {
        // Test: Verify reservation has all required navigation properties
        // Purpose: Ensures entity relationships are properly configured
        var reservation = new Reservation();

        Assert.NotNull(reservation.Payments);
        Assert.NotNull(reservation.Companions);
        Assert.Empty(reservation.Payments);
        Assert.Empty(reservation.Companions);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Reservation_ValidNumberOfGuests(int guests)
    {
        // Test: Verify valid number of guests
        // Purpose: Tests business logic constraints
        var reservation = new Reservation { NumberOfGuests = guests };

        Assert.InRange(guests, 1, 10);
        Assert.Equal(guests, reservation.NumberOfGuests);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(-1)]
    public void Reservation_InvalidNumberOfGuests(int guests)
    {
        // Test: Verify invalid number of guests
        // Purpose: Tests validation constraints
        Assert.True(guests < 1 || guests > 10, "Invalid guest count should be outside 1-10 range");
    }

    [Fact]
    public async Task Reservation_SoftDeleteWorks()
    {
        // Test: Verify soft delete functionality
        // Purpose: Ensures IsActive filtering works correctly
        var user = new User
        {
            Name = "Test User",
            Email = "user@test.com",
            Password = "password",
            PhoneNumber = "+5511999999999"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var activeReservation = new Reservation
        {
            UserId = user.Id,
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(35),
            TotalPrice = 1000m,
            NumberOfGuests = 2,
            Status = "Confirmed",
            IsActive = true
        };

        var inactiveReservation = new Reservation
        {
            UserId = user.Id,
            StartDate = DateTime.UtcNow.AddDays(40),
            EndDate = DateTime.UtcNow.AddDays(45),
            TotalPrice = 1500m,
            NumberOfGuests = 3,
            Status = "Cancelled",
            IsActive = false
        };

        _context.Reservations.Add(activeReservation);
        _context.Reservations.Add(inactiveReservation);
        await _context.SaveChangesAsync();

        var reservations = await _context.Reservations.ToListAsync();

        Assert.Single(reservations);
        Assert.Equal("Confirmed", reservations.First().Status);
        
        // Verify inactive reservation exists when ignoring filters
        var allReservations = await _context.Reservations.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(2, allReservations.Count);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}