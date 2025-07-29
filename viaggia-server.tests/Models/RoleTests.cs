using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Users;
using viaggia_server.Models.UserRoles;

namespace viaggia_server.tests.Models;

public class RoleTests : IDisposable
{
    private readonly AppDbContext _context;

    public RoleTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task Role_CanCreateRole()
    {
        // Test: Verify role creation
        // Purpose: Tests basic role entity functionality
        var role = new Role
        {
            Name = "CLIENT",
            IsActive = true
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var savedRole = await _context.Roles.FirstAsync();

        Assert.Equal("CLIENT", savedRole.Name);
        Assert.True(savedRole.IsActive);
    }

    [Fact]
    public void Role_HasRequiredCollections()
    {
        // Test: Verify role has required navigation properties
        // Purpose: Ensures entity relationships are properly configured
        var role = new Role();

        Assert.NotNull(role.UserRoles);
        Assert.Empty(role.UserRoles);
    }

    [Theory]
    [InlineData("CLIENT")]
    [InlineData("SERVICE_PROVIDER")]
    [InlineData("ATTENDANT")]
    [InlineData("ADMIN")]
    public async Task Role_CanCreateStandardRoles(string roleName)
    {
        // Test: Verify creation of standard application roles
        // Purpose: Tests standard role types used in the application
        var role = new Role
        {
            Name = roleName,
            IsActive = true
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var savedRole = await _context.Roles.FirstAsync();

        Assert.Equal(roleName, savedRole.Name);
        Assert.True(savedRole.IsActive);
    }

    [Fact]
    public async Task UserRole_CanCreateUserRoleAssociation()
    {
        // Test: Verify UserRole junction table functionality
        // Purpose: Tests many-to-many relationship between User and Role
        var user = new User
        {
            Name = "Test User",
            Email = "user@test.com",
            Password = "password",
            PhoneNumber = "+5511999999999"
        };

        var role = new Role
        {
            Name = "CLIENT",
            IsActive = true
        };

        _context.Users.Add(user);
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        var savedUserRole = await _context.UserRoles
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .FirstAsync();

        Assert.Equal(user.Id, savedUserRole.UserId);
        Assert.Equal(role.Id, savedUserRole.RoleId);
        Assert.NotNull(savedUserRole.User);
        Assert.NotNull(savedUserRole.Role);
        Assert.Equal("Test User", savedUserRole.User.Name);
        Assert.Equal("CLIENT", savedUserRole.Role.Name);
    }

    [Fact]
    public async Task UserRole_CompositeKeyWorks()
    {
        // Test: Verify composite key functionality for UserRole
        // Purpose: Ensures composite primary key works correctly
        var user = new User
        {
            Name = "Test User",
            Email = "user@test.com",
            Password = "password",
            PhoneNumber = "+5511999999999"
        };

        var clientRole = new Role { Name = "CLIENT", IsActive = true };
        var adminRole = new Role { Name = "ADMIN", IsActive = true };

        _context.Users.Add(user);
        _context.Roles.Add(clientRole);
        _context.Roles.Add(adminRole);
        await _context.SaveChangesAsync();

        // User can have multiple roles
        var userClientRole = new UserRole
        {
            UserId = user.Id,
            RoleId = clientRole.Id
        };

        var userAdminRole = new UserRole
        {
            UserId = user.Id,
            RoleId = adminRole.Id
        };

        _context.UserRoles.Add(userClientRole);
        _context.UserRoles.Add(userAdminRole);
        await _context.SaveChangesAsync();

        var userRoles = await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == user.Id)
            .ToListAsync();

        Assert.Equal(2, userRoles.Count);
        Assert.Contains(userRoles, ur => ur.Role.Name == "CLIENT");
        Assert.Contains(userRoles, ur => ur.Role.Name == "ADMIN");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}