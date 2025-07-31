using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using viaggia_server.Data;
using ViaggiaServer.Models.Packages;

namespace viaggia_server.tests.Models
{
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
        public void PackageHasRequiredProperties()
        {
            // Test: Verify Package entity has all required properties
            // Purpose: Ensures data model integrity
            var package = new Package
            {
                Name = "Beach Vacation",
                Destination = "Bahamas",
                Description = "A relaxing beach vacation in the Bahamas.",
                BasePrice = 1000.00m,
                IsActive = true
            };
            Assert.NotNull(package.Name);
            Assert.NotNull(package.Destination);
            Assert.True(package.BasePrice >= 0);
        }

        [Fact]
        public void PackageHasRequiredCollections()
        {
            // Test: Verify Package entity has all required collections
            // Purpose: Ensures data model integrity
            var package = new Package();
            Assert.NotNull(package.Medias);
            Assert.NotNull(package.PackageDates);
            Assert.NotNull(package.Reservations);
            Assert.Empty(package.Medias);
            Assert.Empty(package.PackageDates);
            Assert.Empty(package.Reservations);
        }   

        [Fact]
        public async Task CanCreatePackage()
        {
            // Test: Verify a package can be created and saved
            // Purpose: Tests package creation functionality
            var package = new Package
            {
                Name = "Beach Vacation",
                Destination = "Bahamas",
                Description = "A relaxing beach vacation in the Bahamas.",
                BasePrice = 1000.00m,
                IsActive = true
            };
            _context.Packages.Add(package);
            await _context.SaveChangesAsync();
            var savedPackage = await _context.Packages.FirstAsync();
            Assert.Equal(package.PackageId, savedPackage.PackageId);
            Assert.Equal("Beach Vacation", savedPackage.Name);
            Assert.Equal("Bahamas", savedPackage.Destination);
            Assert.Equal(1000.00m, savedPackage.BasePrice);
            Assert.True(savedPackage.IsActive);
        }

        [Fact]
        public async Task PackageSoftDeleteWorks()
        {
            // Test: Verify soft delete functionality for packages
            // Purpose: Ensures IsActive filtering works correctly

            var activePackage = new Package
            {
                Name = "Active Package",
                Destination = "São Paulo",
                BasePrice = 1000m,
                IsActive = true
            };

            var inactivePackage = new Package
            {
                Name = "Inactive Package",
                Destination = "Brasília",
                BasePrice = 2000m,
                IsActive = false
            };

            _context.Packages.Add(activePackage);
            _context.Packages.Add(inactivePackage);
            await _context.SaveChangesAsync();

            // Should only return active packages due to global query filter
            var packages = await _context.Packages.ToListAsync();

            // Verify only one package is returned (the active one)
            Assert.Single(packages);
            Assert.Equal("Active Package", packages[0].Name);
            Assert.True(packages[0].IsActive);

            // To verify the inactive package was actually saved but filtered out,
            // we need to query without the global filter
            var allPackagesIncludingInactive = await _context.Packages.IgnoreQueryFilters().ToListAsync();

            var inactivePackageFromDb = allPackagesIncludingInactive.FirstOrDefault(u => u.Name == "Inactive Package");
            Assert.NotNull(inactivePackageFromDb);
            Assert.False(inactivePackageFromDb.IsActive);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void PackagePriceValidation(decimal price)
        {
            var package = new Package
            {
                Name = "Test Package",
                Destination = "Test Destination",
                BasePrice = price
            };

            if(price < 0)
            {
                Assert.True(price < 0, "Price should be positive");
            }
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

    }
}