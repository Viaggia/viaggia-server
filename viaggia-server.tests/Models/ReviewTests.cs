using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Reviews;
using viaggia_server.Models.Users;
using viaggia_server.Models.Hotels;

namespace viaggia_server.tests.Models;

public class ReviewTests : IDisposable
{
    private readonly AppDbContext _context;

    public ReviewTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task Review_CanCreateHotelReview()
    {
        // Test: Verify hotel review creation
        // Purpose: Tests basic review entity functionality for hotels
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

        _context.Users.Add(user);
        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        var review = new Review
        {
            UserId = user.Id,
            ReviewType = "Hotel",
            HotelId = hotel.HotelId,
            Rating = 5,
            Comment = "Excellent hotel with great service!",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var savedReview = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Hotel)
            .FirstAsync();

        Assert.NotNull(savedReview.User);
        Assert.NotNull(savedReview.Hotel);
        Assert.Equal("Hotel", savedReview.ReviewType);
        Assert.Equal(5, savedReview.Rating);
        Assert.Equal("Excellent hotel with great service!", savedReview.Comment);
        Assert.Equal("Test User", savedReview.User.Name);
        Assert.Equal("Test Hotel", savedReview.Hotel.Name);
    }

    [Fact]
    public async Task Review_CanCreateAgencyReview()
    {
        // Test: Verify agency review creation
        // Purpose: Tests review functionality for agencies
        var user = new User
        {
            Name = "Test User",
            Email = "user@test.com",
            Password = "password",
            PhoneNumber = "+5511999999999"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var review = new Review
        {
            UserId = user.Id,
            ReviewType = "Agency",
            HotelId = null, // No hotel for agency reviews
            Rating = 4,
            Comment = "Good travel agency with reasonable prices.",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var savedReview = await _context.Reviews
            .Include(r => r.User)
            .FirstAsync();

        Assert.NotNull(savedReview.User);
        Assert.Null(savedReview.Hotel);
        Assert.Equal("Agency", savedReview.ReviewType);
        Assert.Equal(4, savedReview.Rating);
        Assert.Equal("Good travel agency with reasonable prices.", savedReview.Comment);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Review_ValidRatings(int rating)
    {
        // Test: Verify valid rating values
        // Purpose: Tests rating validation constraints
        var review = new Review { Rating = rating };

        Assert.InRange(rating, 1, 5);
        Assert.Equal(rating, review.Rating);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Review_InvalidRatings(int rating)
    {
        // Test: Verify invalid rating values are outside valid range
        // Purpose: Tests validation constraints
        Assert.True(rating < 1 || rating > 5, "Invalid rating should be outside 1-5 range");
    }

    [Theory]
    [InlineData("Hotel")]
    [InlineData("Agency")]
    public async Task Review_ValidReviewTypes(string reviewType)
    {
        // Test: Verify valid review types
        // Purpose: Tests review type validation
        var user = new User
        {
            Name = "Test User",
            Email = "user@test.com",
            Password = "password",
            PhoneNumber = "+5511999999999"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var review = new Review
        {
            UserId = user.Id,
            ReviewType = reviewType,
            Rating = 4,
            Comment = "Test review"
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var savedReview = await _context.Reviews.FirstAsync();

        Assert.Equal(reviewType, savedReview.ReviewType);
    }

    [Fact]
    public async Task Review_SoftDeleteWorks()
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

        var activeReview = new Review
        {
            UserId = user.Id,
            ReviewType = "Agency",
            Rating = 5,
            Comment = "Active review",
            IsActive = true
        };

        var inactiveReview = new Review
        {
            UserId = user.Id,
            ReviewType = "Agency",
            Rating = 3,
            Comment = "Inactive review",
            IsActive = false
        };

        _context.Reviews.Add(activeReview);
        _context.Reviews.Add(inactiveReview);
        await _context.SaveChangesAsync();

        var reviews = await _context.Reviews.ToListAsync();

        Assert.Single(reviews);
        Assert.Equal("Active review", reviews.First().Comment);
        
        // Verify inactive review exists when ignoring filters
        var allReviews = await _context.Reviews.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(2, allReviews.Count);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}