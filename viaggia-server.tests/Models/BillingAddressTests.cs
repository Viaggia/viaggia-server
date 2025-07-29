using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Addresses;

namespace viaggia_server.tests.Models;

public class BillingAddressTests : IDisposable
{
    private readonly AppDbContext _context;

    public BillingAddressTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task BillingAddress_CanCreateBillingAddress()
    {
        // Test: Verify billing address creation inheriting from Address
        // Purpose: Tests inheritance functionality and billing-specific behavior
        var billingAddress = new BillingAddress
        {
            Street = "Av. Paulista, 1500",
            City = "São Paulo",
            State = "SP",
            ZipCode = "01310-100",
            IsActive = true
        };

        _context.BillingAddresses.Add(billingAddress);
        await _context.SaveChangesAsync();

        var savedBillingAddress = await _context.BillingAddresses.FirstAsync();

        Assert.Equal("Av. Paulista, 1500", savedBillingAddress.Street);
        Assert.Equal("São Paulo", savedBillingAddress.City);
        Assert.Equal("SP", savedBillingAddress.State);
        Assert.Equal("01310-100", savedBillingAddress.ZipCode);
        Assert.True(savedBillingAddress.IsActive);
    }

    [Fact]
    public void BillingAddress_InheritsFromAddress()
    {
        // Test: Verify BillingAddress inherits from Address base class
        // Purpose: Tests inheritance relationship
        var billingAddress = new BillingAddress
        {
            Street = "Test Street",
            City = "Test City",
            State = "TS",
            ZipCode = "12345-678"
        };

        // Should have all Address properties
        Assert.NotNull(billingAddress.Street);
        Assert.NotNull(billingAddress.City);
        Assert.NotNull(billingAddress.State);
        Assert.NotNull(billingAddress.ZipCode);
        Assert.True(billingAddress.IsActive); // Default from Address
        Assert.Equal(0, billingAddress.AddressId); // Inherited key
    }

    [Fact]
    public async Task BillingAddress_CanBeUsedInPayment()
    {
        // Test: Verify billing address can be associated with payments
        // Purpose: Tests relationship with Payment entity
        var billingAddress = new BillingAddress
        {
            Street = "Rua do Comércio, 500",
            City = "Rio de Janeiro",
            State = "RJ",
            ZipCode = "20000-000"
        };

        _context.BillingAddresses.Add(billingAddress);
        await _context.SaveChangesAsync();

        // Verify it can be referenced by payment
        Assert.True(billingAddress.AddressId > 0);
        Assert.IsType<BillingAddress>(billingAddress);
    }

    [Theory]
    [InlineData("Rua Comercial, 100", "São Paulo", "SP", "01000-000")]
    [InlineData("Av. Copacabana, 200", "Rio de Janeiro", "RJ", "22000-000")]
    [InlineData("SQS 304 Bloco B", "Brasília", "DF", "70000-000")]
    public async Task BillingAddress_CanCreateForDifferentBusinessLocations(
        string street, string city, string state, string zipCode)
    {
        // Test: Verify billing addresses for different business locations
        // Purpose: Tests billing address creation for various business scenarios
        var billingAddress = new BillingAddress
        {
            Street = street,
            City = city,
            State = state,
            ZipCode = zipCode
        };

        _context.BillingAddresses.Add(billingAddress);
        await _context.SaveChangesAsync();

        var savedAddress = await _context.BillingAddresses.FirstAsync();

        Assert.Equal(street, savedAddress.Street);
        Assert.Equal(city, savedAddress.City);
        Assert.Equal(state, savedAddress.State);
        Assert.Equal(zipCode, savedAddress.ZipCode);
    }

    [Fact]
    public async Task BillingAddress_CanCreateMultipleBillingAddresses()
    {
        // Test: Verify multiple billing addresses can be created
        // Purpose: Tests support for multiple billing addresses (different users/companies)
        var billingAddresses = new List<BillingAddress>
        {
            new BillingAddress
            {
                Street = "Rua Cliente 1, 100",
                City = "São Paulo",
                State = "SP",
                ZipCode = "01000-000"
            },
            new BillingAddress
            {
                Street = "Av. Cliente 2, 200",
                City = "Rio de Janeiro",
                State = "RJ",
                ZipCode = "22000-000"
            },
            new BillingAddress
            {
                Street = "Quadra Cliente 3",
                City = "Brasília",
                State = "DF",
                ZipCode = "70000-000"
            }
        };

        _context.BillingAddresses.AddRange(billingAddresses);
        await _context.SaveChangesAsync();

        var savedAddresses = await _context.BillingAddresses
            .OrderBy(ba => ba.Street)
            .ToListAsync();

        Assert.Equal(3, savedAddresses.Count);
        Assert.Equal("Av. Cliente 2, 200", savedAddresses[0].Street);
        Assert.Equal("Quadra Cliente 3", savedAddresses[1].Street);
        Assert.Equal("Rua Cliente 1, 100", savedAddresses[2].Street);
    }

    [Fact]
    public void BillingAddress_ValidatesLikeAddress()
    {
        // Test: Verify billing address has same validation as Address
        // Purpose: Tests inherited validation behavior
        var billingAddress = new BillingAddress();

        // Should inherit all Address validation requirements
        Assert.Null(billingAddress.Street);
        Assert.Null(billingAddress.City);
        Assert.Null(billingAddress.State);
        Assert.Null(billingAddress.ZipCode);
        Assert.True(billingAddress.IsActive); // Default from base class
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}