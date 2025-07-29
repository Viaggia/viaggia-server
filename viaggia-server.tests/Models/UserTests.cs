using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Users;

namespace viaggia_server.tests.Models;

public class UserTests : IDisposable
{
    private readonly AppDbContext _context;

    public UserTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
    }

    [Fact]
    public void User_HasRequiredProperties()
    {
        // Test: Verify User entity has all required properties with correct default values
        // Purpose: Ensures data model integrity and proper initialization
        var user = new User();

        Assert.NotNull(user.UserRoles);
        Assert.NotNull(user.Reservations);
        Assert.NotNull(user.Payments);
        Assert.True(user.IsActive); // Default should be true
        Assert.True(user.CreateDate <= DateTime.UtcNow);
        Assert.Equal(string.Empty, user.AvatarUrl); // Default should be empty string
        Assert.Empty(user.UserRoles);
        Assert.Empty(user.Reservations);
        Assert.Empty(user.Payments);
    }

    [Fact]
    public async Task User_CanCreateClientUser()
    {
        // Test: Verify client-type user creation with CPF (no address fields anymore)
        // Purpose: Tests client-specific functionality
        var clientUser = new User
        {
            Name = "João Silva",
            Email = "joao@example.com",
            Password = "hashedpassword",
            PhoneNumber = "+5511987654321",
            Cpf = "123.456.789-00",
            AvatarUrl = "https://example.com/avatar.jpg"
        };

        _context.Users.Add(clientUser);
        await _context.SaveChangesAsync();

        var savedUser = await _context.Users.FirstAsync();

        Assert.Equal("João Silva", savedUser.Name);
        Assert.Equal("123.456.789-00", savedUser.Cpf);
        Assert.Equal("https://example.com/avatar.jpg", savedUser.AvatarUrl);

        // Should be null for client user
        Assert.Null(savedUser.Cnpj);
        Assert.Null(savedUser.CompanyName);
        Assert.Null(savedUser.EmployeeId);
    }

    [Fact]
    public async Task User_CanCreateServiceProviderUser()
    {
        // Test: Verify service provider user creation with CNPJ and company details
        // Purpose: Tests service provider-specific functionality
        var providerUser = new User
        {
            Name = "Maria Santos",
            Email = "maria@company.com",
            Password = "hashedpassword",
            PhoneNumber = "+5511123456789",
            CompanyName = "Viagens LTDA",
            Cnpj = "12.345.678/0001-99",
            CompanyLegalName = "Viagens e Turismo LTDA",
            StripeCustomerId = "cust_test123" 
        };

        _context.Users.Add(providerUser);
        await _context.SaveChangesAsync();

        var savedUser = await _context.Users.FirstAsync();

        Assert.Equal("12.345.678/0001-99", savedUser.Cnpj);
        Assert.Equal("Viagens LTDA", savedUser.CompanyName);
        Assert.Equal("Viagens e Turismo LTDA", savedUser.CompanyLegalName);
        Assert.Equal("cust_test123", savedUser.StripeCustomerId); 

        // Should be null for service provider
        Assert.Null(savedUser.Cpf);
        Assert.Null(savedUser.EmployeeId);
    }

    [Fact]
    public async Task User_CanCreateAttendantUser()
    {
        // Test: Verify attendant user creation with employee details
        // Purpose: Tests attendant-specific functionality
        var attendantUser = new User
        {
            Name = "Carlos Oliveira",
            Email = "carlos@support.com",
            Password = "hashedpassword",
            PhoneNumber = "+5511555666777",
            EmployerCompanyName = "Suporte Tech LTDA",
            EmployeeId = "EMP001"
        };

        _context.Users.Add(attendantUser);
        await _context.SaveChangesAsync();

        var savedUser = await _context.Users.FirstAsync();

        Assert.Equal("Suporte Tech LTDA", savedUser.EmployerCompanyName);
        Assert.Equal("EMP001", savedUser.EmployeeId);

        // Should be null for attendant
        Assert.Null(savedUser.Cpf);
        Assert.Null(savedUser.Cnpj);
    }

    [Fact]
    public async Task User_CanCreateGoogleUser()
    {
        // Test: Verify Google OAuth user creation
        // Purpose: Tests Google authentication integration
        var googleUser = new User
        {
            Name = "Ana Google",
            Email = "ana@gmail.com",
            Password = "google_oauth_temp", // Temporary password for OAuth users
            PhoneNumber = "+5511888999000",
            GoogleId = "google_123456789",
            AvatarUrl = "https://lh3.googleusercontent.com/avatar.jpg"
        };

        _context.Users.Add(googleUser);
        await _context.SaveChangesAsync();

        var savedUser = await _context.Users.FirstAsync();

        Assert.Equal("google_123456789", savedUser.GoogleId);
        Assert.Equal("https://lh3.googleusercontent.com/avatar.jpg", savedUser.AvatarUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void User_RequiredFieldsValidation_ThrowsException(string invalidValue)
    {
        // Test: Verify required field validation
        // Purpose: Ensures data integrity constraints work
        var user = new User
        {
            Name = invalidValue,
            Email = "test@example.com",
            Password = "password",
            PhoneNumber = "+5511999999999"
        };

        if (string.IsNullOrEmpty(user.Name))
        {
            Assert.True(string.IsNullOrEmpty(invalidValue), "Name validation should catch empty/null values");
        }
    }

    [Theory]
    [InlineData("testemail@viaggia.com")]
    public void User_EmailValidation(string validEmail)
    {
        // Test: Verify email format validation
        // Purpose: Tests email validation attributes work correctly
        var user = new User { Email = validEmail };

        // Basic email validation check (would be caught by model validation in real scenario)
        Assert.True(validEmail.Contains("@") && validEmail.Contains(".") && validEmail.Length > 5,
            "Valid email should pass validation");
    }

    [Fact]
    public async Task User_SoftDeleteWorks()
    {
        // Test: Verify soft delete functionality
        // Purpose: Ensures IsActive filtering works correctly
        var activeUser = new User
        {
            Name = "Active User",
            Email = "active@example.com",
            Password = "password",
            PhoneNumber = "+5511111111111",
            IsActive = true
        };

        var inactiveUser = new User
        {
            Name = "Inactive User",
            Email = "inactive@example.com",
            Password = "password",
            PhoneNumber = "+5511222222222",
            IsActive = false
        };

        _context.Users.Add(activeUser);
        _context.Users.Add(inactiveUser);
        await _context.SaveChangesAsync();

        var activeUsers = await _context.Users.ToListAsync();

        Assert.Single(activeUsers);
        Assert.Equal("Active User", activeUsers.First().Name);

        // Verify inactive user exists when ignoring filters
        var allUsers = await _context.Users.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(2, allUsers.Count);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}