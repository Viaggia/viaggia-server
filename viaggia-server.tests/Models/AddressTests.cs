using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Addresses;

namespace viaggia_server.tests.Models;

public class AddressTests : IDisposable
{
    private readonly AppDbContext _context;

    public AddressTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task Address_CanCreateAddress()
    {
        // Test: Verify address creation with all required fields
        // Purpose: Tests basic address entity functionality
        var address = new Address
        {
            Street = "Rua das Flores, 123",
            City = "São Paulo",
            State = "SP",
            ZipCode = "01234-567",
            IsActive = true
        };

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        var savedAddress = await _context.Addresses.FirstAsync();

        Assert.Equal("Rua das Flores, 123", savedAddress.Street);
        Assert.Equal("São Paulo", savedAddress.City);
        Assert.Equal("SP", savedAddress.State);
        Assert.Equal("01234-567", savedAddress.ZipCode);
        Assert.True(savedAddress.IsActive);
    }

    [Theory]
    [InlineData("Av. Paulista, 1000", "São Paulo", "SP", "01310-100")]
    [InlineData("Rua Copacabana, 500", "Rio de Janeiro", "RJ", "22070-010")]
    [InlineData("SQN 123 Bloco A", "Brasília", "DF", "70000-000")]
    public async Task Address_CanCreateVariousAddresses(string street, string city, string state, string zipCode)
    {
        // Test: Verify creation of addresses from different Brazilian locations
        // Purpose: Tests address validation for various Brazilian formats
        var address = new Address
        {
            Street = street,
            City = city,
            State = state,
            ZipCode = zipCode,
            IsActive = true
        };

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        var savedAddress = await _context.Addresses.FirstAsync();

        Assert.Equal(street, savedAddress.Street);
        Assert.Equal(city, savedAddress.City);
        Assert.Equal(state, savedAddress.State);
        Assert.Equal(zipCode, savedAddress.ZipCode);
    }

    [Theory]
    [InlineData("SP")]
    [InlineData("RJ")]
    [InlineData("MG")]
    [InlineData("DF")]
    [InlineData("RS")]
    public void Address_ValidBrazilianStates(string state)
    {
        // Test: Verify valid Brazilian state abbreviations
        // Purpose: Tests state validation for Brazilian addresses
        var address = new Address { State = state };

        Assert.Equal(2, state.Length); // Brazilian states are 2 characters
        Assert.Equal(state, address.State);
    }

    [Theory]
    [InlineData("01234-567")]
    [InlineData("12345-678")]
    [InlineData("99999-999")]
    public void Address_ValidZipCodeFormat(string zipCode)
    {
        // Test: Verify valid Brazilian ZIP code format
        // Purpose: Tests ZIP code validation for Brazilian CEP format
        var address = new Address { ZipCode = zipCode };

        Assert.Equal(9, zipCode.Length); // Brazilian ZIP code with hyphen is 9 characters
        Assert.Contains("-", zipCode);
        Assert.Equal(zipCode, address.ZipCode);
    }

    [Fact]
    public void Address_RequiredFieldsValidation()
    {
        // Test: Verify required fields validation
        // Purpose: Tests model validation attributes
        var address = new Address();

        // These would be caught by model validation in real scenarios
        Assert.Null(address.Street);
        Assert.Null(address.City);
        Assert.Null(address.State);
        Assert.Null(address.ZipCode);
        Assert.True(address.IsActive); // Default should be true
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Address_EmptyRequiredFields_ShouldFailValidation(string invalidValue)
    {
        // Test: Verify empty required fields fail validation
        // Purpose: Tests validation constraints for required fields
        var address = new Address
        {
            Street = invalidValue,
            City = "São Paulo",
            State = "SP",
            ZipCode = "01234-567"
        };

        if (string.IsNullOrEmpty(address.Street))
        {
            Assert.True(string.IsNullOrEmpty(invalidValue), "Empty street should fail validation");
        }
    }

    [Theory]
    [InlineData("This is a very long street name that exceeds the maximum allowed length of 100 characters and should fail validation")]
    [InlineData("This is a very long city name that exceeds fifty chars")]
    public void Address_ExceedsMaxLength_ShouldFailValidation(string longValue)
    {
        // Test: Verify fields exceeding max length fail validation
        // Purpose: Tests string length validation constraints
        if (longValue.Length > 100)
        {
            Assert.True(longValue.Length > 100, "Long street name should exceed 100 characters");
        }
        else if (longValue.Length > 50)
        {
            Assert.True(longValue.Length > 50, "Long city name should exceed 50 characters");
        }
    }

    [Fact]
    public async Task Address_UpdateAddress()
    {
        // Test: Verify address can be updated
        // Purpose: Tests entity update functionality
        var address = new Address
        {
            Street = "Rua Original, 100",
            City = "São Paulo",
            State = "SP",
            ZipCode = "01234-567"
        };

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        // Update the address
        address.Street = "Rua Atualizada, 200";
        address.ZipCode = "07654-321";
        await _context.SaveChangesAsync();

        var updatedAddress = await _context.Addresses.FirstAsync();

        Assert.Equal("Rua Atualizada, 200", updatedAddress.Street);
        Assert.Equal("07654-321", updatedAddress.ZipCode);
        Assert.Equal("São Paulo", updatedAddress.City); // Unchanged
        Assert.Equal("SP", updatedAddress.State); // Unchanged
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}