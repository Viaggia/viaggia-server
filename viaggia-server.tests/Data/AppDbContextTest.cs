using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Users;

namespace viaggia_server.tests;

public class AppDbContextTest : IDisposable
{
    private readonly AppDbContext _context;

    public AppDbContextTest()
    {
        // Create InMemory database options for testing
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
    }

    [Fact]
    public void CanConnectToDB()
    {
        // Test: Verify that the context can connect to the database
        // Purpose: Ensures basic database connectivity for CI/CD pipeline
        Assert.True(_context.Database.CanConnect());
    }

    [Fact]
    public void CanCreateDB()
    {
        // Test: Verify that the database can be created
        // Purpose: Ensures database schema creation works in CI/CD environment
        var result = _context.Database.EnsureCreated();
        Assert.True(result || _context.Database.CanConnect());
    }

    [Fact]
    public void DbSetsAreNotNull()
    {
        // Test: Verify all DbSets are properly initialized
        // Purpose: Ensures all entity sets are available for operations
        Assert.NotNull(_context.Users);
        Assert.NotNull(_context.Roles);
        Assert.NotNull(_context.UserRoles);
        Assert.NotNull(_context.Packages);
        Assert.NotNull(_context.PackageDates);
        Assert.NotNull(_context.Hotels);
        Assert.NotNull(_context.RoomTypes);
        Assert.NotNull(_context.HotelDates);
        Assert.NotNull(_context.Reservations);
        Assert.NotNull(_context.Payments);
        Assert.NotNull(_context.Medias);
        Assert.NotNull(_context.Reviews);
        Assert.NotNull(_context.Companions);
    }

    [Fact]
    public async Task CanSaveAndReturnUser()
    {
        // Test: Verify basic CRUD operations work
        // Purpose: Ensures Entity Framework operations work correctly with InMemory provider
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            Password = "password",
            PhoneNumber = "+5511999999999",
            Cpf = "123.456.789-00",
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var retrievedUser = await _context.Users.SingleOrDefaultAsync(u => u.Id == 1);

        Assert.NotNull(retrievedUser);
        Assert.Equal(1, retrievedUser.Id);
        Assert.Equal("Test User", retrievedUser.Name);
        Assert.Equal("test@example.com", retrievedUser.Email);
        Assert.Equal("password", retrievedUser.Password);
        Assert.Equal("+5511999999999", retrievedUser.PhoneNumber);
        Assert.Equal("123.456.789-00", retrievedUser.Cpf);
    }

    [Fact]
    public async Task SoftDeleteWorks()
    {
        // Test: Verify soft delete global query filters work correctly
        // Purpose: Ensures IsActive filtering works as expected
        var activeUser = new User
        { 
            Id= 1,
            Name = "Active User",
            Email = "active@example.com",
            Password = "password",
            PhoneNumber = "+5511111111111",
            IsActive = true
        };

        var inactiveUser = new User
        {
            Id = 2,
            Name = "Inactive User",
            Email = "inactive@example.com",
            Password = "password",
            PhoneNumber = "+5511999999999",
            IsActive = false
        };

        _context.Users.Add(activeUser); 
        _context.Users.Add(inactiveUser);
        await _context.SaveChangesAsync();

        // Should only return active users due to global query filter
        var users = await _context.Users.ToListAsync();

        // Verify only one user is returned (the active one)
        Assert.Single(users);
        Assert.Equal("Active User", users[0].Name);
        Assert.True(users[0].IsActive);

        // To verify the inactive user was actually saved but filtered out,
        // we need to query without the global filter
        var allUsersIncludingInactive = await _context.Users.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(2, allUsersIncludingInactive.Count);
        
        var inactiveUserFromDb = allUsersIncludingInactive.FirstOrDefault(u => u.Email == "inactive@example.com");
        Assert.NotNull(inactiveUserFromDb);
        Assert.False(inactiveUserFromDb.IsActive);
    }

    public void Dispose()
    {
        // Clean up the in-memory database after tests
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
