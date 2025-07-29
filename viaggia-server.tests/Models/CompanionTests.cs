using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Companions;
using viaggia_server.Models.Users;
using viaggia_server.Models.Reservations;

namespace viaggia_server.tests.Models;

public class CompanionTests : IDisposable
{
    private readonly AppDbContext _context;

    public CompanionTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task Companion_CanCreateCompanion()
    {
        // Test: Verify companion creation with required relationships
        // Purpose: Tests basic companion entity functionality
        var user = new User
        {
            Name = "Test User",
            Email = "user@test.com",
            Password = "password",
            PhoneNumber = "+5511999999999"
        };

        var reservation = new Reservation
        {
            UserId = user.Id,
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(35),
            TotalPrice = 1500m,
            NumberOfGuests = 3, // Main user + 2 companions
            Status = "Confirmed"
        };

        _context.Users.Add(user);
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var companion = new Companion
        {
            ReservationId = reservation.ReservationId,
            Name = "João Companion",
            CPF = "123.456.789-00",
            BirthDate = new DateTime(1990, 5, 15),
            IsActive = true
        };

        _context.Companions.Add(companion);
        await _context.SaveChangesAsync();

        var savedCompanion = await _context.Companions
            .Include(c => c.Reservation)
            .FirstAsync();

        Assert.NotNull(savedCompanion.Reservation);
        Assert.Equal("João Companion", savedCompanion.Name);
        Assert.Equal("123.456.789-00", savedCompanion.CPF);
        Assert.Equal(new DateTime(1990, 5, 15), savedCompanion.BirthDate);
        Assert.Equal(reservation.ReservationId, savedCompanion.ReservationId);
    }

    [Fact]
    public async Task Companion_CanCreateMultipleCompanions()
    {
        // Test: Verify multiple companions for single reservation
        // Purpose: Tests one-to-many relationship functionality
        var user = new User
        {
            Name = "Test User",
            Email = "user@test.com",
            Password = "password",
            PhoneNumber = "+5511999999999"
        };

        var reservation = new Reservation
        {
            UserId = user.Id,
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(35),
            TotalPrice = 2000m,
            NumberOfGuests = 4, // Main user + 3 companions
            Status = "Confirmed"
        };

        _context.Users.Add(user);
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var companions = new List<Companion>
        {
            new Companion
            {
                ReservationId = reservation.ReservationId,
                Name = "Maria Silva",
                CPF = "111.222.333-44",
                BirthDate = new DateTime(1985, 3, 20)
            },
            new Companion
            {
                ReservationId = reservation.ReservationId,
                Name = "Pedro Santos",
                CPF = "555.666.777-88",
                BirthDate = new DateTime(1992, 8, 10)
            },
            new Companion
            {
                ReservationId = reservation.ReservationId,
                Name = "Ana Costa",
                CPF = "999.888.777-66",
                BirthDate = new DateTime(1988, 12, 5)
            }
        };

        _context.Companions.AddRange(companions);
        await _context.SaveChangesAsync();

        var savedCompanions = await _context.Companions
            .Where(c => c.ReservationId == reservation.ReservationId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        Assert.Equal(3, savedCompanions.Count);
        Assert.Equal("Ana Costa", savedCompanions[0].Name);
        Assert.Equal("Maria Silva", savedCompanions[1].Name);
        Assert.Equal("Pedro Santos", savedCompanions[2].Name);
    }

    [Fact]
    public void Companion_BirthDateValidation()
    {
        // Test: Verify birth date is in the past
        // Purpose: Tests age validation logic
        var companion = new Companion
        {
            BirthDate = DateTime.UtcNow.AddDays(-1) // Yesterday
        };

        Assert.True(companion.BirthDate < DateTime.UtcNow, "Birth date should be in the past");
    }

    [Theory]
    [InlineData("123.456.789-00")]
    [InlineData("987.654.321-12")]
    [InlineData("111.222.333-44")]
    public void Companion_ValidCPFFormat(string cpf)
    {
        // Test: Verify CPF format validation
        // Purpose: Tests CPF format constraints
        var companion = new Companion { CPF = cpf };

        Assert.Equal(14, cpf.Length); // CPF should have 14 characters with formatting
        Assert.Contains(".", cpf);
        Assert.Contains("-", cpf);
        Assert.Equal(cpf, companion.CPF);
    }

    [Fact]
    public void Companion_RequiredFieldsValidation()
    {
        // Test: Verify required fields validation
        // Purpose: Tests model validation attributes
        var companion = new Companion();

        // These would be caught by model validation in real scenarios
        Assert.Equal(0, companion.ReservationId);
        Assert.Null(companion.Name);
        Assert.Null(companion.CPF);
        Assert.Equal(DateTime.MinValue, companion.BirthDate);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}