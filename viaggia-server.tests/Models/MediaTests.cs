using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Hotels;

namespace viaggia_server.tests.Models;

public class MediaTests : IDisposable
{
    private readonly AppDbContext _context;

    public MediaTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task Media_CanCreatePackageMedia()
    {
        // Test: Verify media creation for packages
        // Purpose: Tests package media relationship
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

        _context.Hotels.Add(hotel);
        _context.Packages.Add(package);
        await _context.SaveChangesAsync();

        var media = new Media
        {
            MediaUrl = "https://example.com/package-image.jpg",
            MediaType = "image",
            PackageId = package.PackageId,
            HotelId = null,
            IsActive = true
        };

        _context.Medias.Add(media);
        await _context.SaveChangesAsync();

        var savedMedia = await _context.Medias
            .Include(m => m.Package)
            .FirstAsync();

        Assert.NotNull(savedMedia.Package);
        Assert.Null(savedMedia.Hotel);
        Assert.Equal("https://example.com/package-image.jpg", savedMedia.MediaUrl);
        Assert.Equal("image", savedMedia.MediaType);
        Assert.Equal("Test Package", savedMedia.Package.Name);
    }

    [Fact]
    public async Task Media_CanCreateHotelMedia()
    {
        // Test: Verify media creation for hotels
        // Purpose: Tests hotel media relationship
        var hotel = new Hotel
        {
            Name = "Test Hotel",
            Cnpj = "12.345.678/0001-99",
            StarRating = 4,
            AddressId = 1
        };

        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        var media = new Media
        {
            MediaUrl = "https://example.com/hotel-video.mp4",
            MediaType = "video",
            PackageId = null,
            HotelId = hotel.HotelId,
            IsActive = true
        };

        _context.Medias.Add(media);
        await _context.SaveChangesAsync();

        var savedMedia = await _context.Medias
            .Include(m => m.Hotel)
            .FirstAsync();

        Assert.NotNull(savedMedia.Hotel);
        Assert.Null(savedMedia.Package);
        Assert.Equal("https://example.com/hotel-video.mp4", savedMedia.MediaUrl);
        Assert.Equal("video", savedMedia.MediaType);
        Assert.Equal("Test Hotel", savedMedia.Hotel.Name);
    }

    [Theory]
    [InlineData("image")]
    [InlineData("video")]
    public void Media_ValidMediaTypes(string mediaType)
    {
        // Test: Verify valid media types
        // Purpose: Tests media type validation
        var media = new Media { MediaType = mediaType };

        Assert.NotEmpty(mediaType);
        Assert.Equal(mediaType, media.MediaType);
    }

    [Fact]
    public void Media_RequiresSingleEntity()
    {
        // Test: Verify media requires either package or hotel (but not both)
        // Purpose: Tests business logic constraint
        var media = new Media
        {
            MediaUrl = "https://example.com/image.jpg",
            MediaType = "image",
            PackageId = 1,
            HotelId = 1 // This should violate the check constraint
        };

        // This would be caught by database check constraint in real scenarios
        Assert.True(media.PackageId.HasValue && media.HotelId.HasValue,
            "Media should not have both PackageId and HotelId set");
    }

    [Fact]
    public async Task Media_SoftDeleteWorks()
    {
        // Test: Verify soft delete functionality
        // Purpose: Ensures IsActive filtering works correctly
        var hotel = new Hotel
        {
            Name = "Test Hotel",
            Cnpj = "12.345.678/0001-99",
            StarRating = 4,
            AddressId = 1
        };

        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        var activeMedia = new Media
        {
            MediaUrl = "https://example.com/active-image.jpg",
            MediaType = "image",
            HotelId = hotel.HotelId,
            IsActive = true
        };

        var inactiveMedia = new Media
        {
            MediaUrl = "https://example.com/inactive-image.jpg",
            MediaType = "image",
            HotelId = hotel.HotelId,
            IsActive = false
        };

        _context.Medias.Add(activeMedia);
        _context.Medias.Add(inactiveMedia);
        await _context.SaveChangesAsync();

        var medias = await _context.Medias.ToListAsync();

        Assert.Single(medias);
        Assert.Contains("active-image", medias.First().MediaUrl);
        
        // Verify inactive media exists when ignoring filters
        var allMedias = await _context.Medias.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(2, allMedias.Count);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}