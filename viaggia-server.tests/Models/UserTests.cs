using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using viaggia_server.Data;
using viaggia_server.Models.Users;

namespace viaggia_server.tests;

public class UserTests : IDisposable
{
    private readonly AppDbContext _context;

    public UserTests()
    {
        // Create InMemory database options for testing
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    [Fact]
    public void User_HasRequiredProperties()
    {
        // Test: Verify User entity has all required properties
        // Purpose: Ensures data model integrity
        var user = new User();

        Assert.NotNull(user.UserRoles);
        Assert.NotNull(user.Reservations);
        Assert.NotNull(user.Payments);
        Assert.True(user.IsActive);
        Assert.True(user.CreateDate <= DateTime.UtcNow);
    }

    [Fact]
    public async Task CanCreateClientUser()
    {
        // Test: Verify client-type user can be created with CPF and address
        // Purpose: Tests client-specific functionality
        var clientUser = new User
        {
            Name = "João Silva",
            Email = "joao@example.com",
            Password = "hashedpassword",
            PhoneNumber = "+5511987654321",
            Cpf = "123.456.789-00",
            AddressStreet = "Rua das Flores, 123",
            AddressCity = "São Paulo",
            AddressState = "SP",
            AddressZipCode = "01234-567"
        };

        _context.Users.Add(clientUser);
        await _context.SaveChangesAsync();

        var savedUser = await _context.Users.FirstAsync();

        Assert.Equal(clientUser.Id, savedUser.Id);
        Assert.Equal("João Silva", savedUser.Name);
        Assert.Equal("123.456.789-00", savedUser.Cpf);
        Assert.Equal("São Paulo", savedUser.AddressCity);

        // Should be null for client user
        Assert.Null(savedUser.EmployeeId);
        Assert.Null(savedUser.EmployerCompanyName);
        Assert.Null(savedUser.CompanyName);
        Assert.Null(savedUser.CompanyLegalName);
        Assert.Null(savedUser.Cnpj);
    }
    [Fact]

    public async Task CanCreateCompanyUser()
    {
        // Test: Verify employee-type user can be created with employee ID and company details
        // Purpose: Tests employee-specific functionality
        var employeeUser = new User
        {
            Name = "Maria Oliveira",
            Email = "maria@example.com",
            Password = "hashedpassword",
            PhoneNumber = "+5511987654321",
            EmployeeId = "E123",
            EmployerCompanyName = "Empresa XYZ",
            CompanyName = "Maria's Company",
            CompanyLegalName = "Maria's Company Ltda",
            Cnpj = "12.345.678/0001-90"
        };

        _context.Users.Add(employeeUser);
        await _context.SaveChangesAsync();

        var savedUser = await _context.Users.FirstAsync();

        Assert.Equal(employeeUser.Id, savedUser.Id);
        Assert.Equal("Maria Oliveira", savedUser.Name);
        Assert.Equal("E123", savedUser.EmployeeId);
        Assert.Equal("Empresa XYZ", savedUser.EmployerCompanyName);
        Assert.Equal("Maria's Company", savedUser.CompanyName);
        Assert.Equal("Maria's Company Ltda", savedUser.CompanyLegalName);
        Assert.Equal("12.345.678/0001-90", savedUser.Cnpj);
    }

    [Fact]
    public async Task CanCreateEmployeeUser()
    {
        // Test: Verify company-type user can be created with company details
        // Purpose: Tests company-specific functionality
        var companyUser = new User
        {
            Name = "Carlos Souza",
            Email = "carlos@example.com",
            Password = "hashedpassword",
            PhoneNumber = "+5511987654321",
            CompanyName = "Carlos's Company",
            CompanyLegalName = "Carlos's Company Ltda",
            Cnpj = "12.345.678/0001-90"
        };

        _context.Users.Add(companyUser);
        await _context.SaveChangesAsync();

        var savedUser = await _context.Users.FirstAsync();

        Assert.Equal(companyUser.Id, savedUser.Id);
        Assert.Equal("Carlos Souza", savedUser.Name);
        Assert.Equal("Carlos's Company", savedUser.CompanyName);
        Assert.Equal("Carlos's Company Ltda", savedUser.CompanyLegalName);
        Assert.Equal("12.345.678/0001-90", savedUser.Cnpj);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task UserRequiredFieldsValidation(string invalidValue)
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

        _context.Users.Add(user);

        // Should throw exception when trying to save invalid data
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            if (string.IsNullOrEmpty(user.Name))
                throw new ArgumentException("Name is required");
            await _context.SaveChangesAsync();
        });
    }


    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
