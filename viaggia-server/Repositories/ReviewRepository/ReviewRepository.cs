using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Reviews;

namespace viaggia_server.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReviewRepository> _logger;

        public ReviewRepository(AppDbContext context, ILogger<ReviewRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Review> CreateReviewAsync(Review review)
        {
            try
            {
                _logger.LogInformation("Creating review for HotelId: {HotelId}, UserId: {UserId}", review.HotelId, review.UserId);
                await _context.Reviews.AddAsync(review);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Review created successfully: {ReviewId}", review.ReviewId);
                return review;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for HotelId: {HotelId}", review.HotelId);
                throw;
            }
        }

        public async Task<IEnumerable<Review>> GetReviewsByHotelIdAsync(int hotelId)
        {
            try
            {
                _logger.LogInformation("Fetching reviews for HotelId: {HotelId}", hotelId);
                var reviews = await _context.Reviews
                    .Where(r => r.HotelId == hotelId && r.IsActive)
                    .Include(r => r.User)
                    .ToListAsync();
                _logger.LogInformation("Found {Count} reviews for HotelId: {HotelId}", reviews.Count, hotelId);
                return reviews;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching reviews for HotelId: {HotelId}", hotelId);
                throw;
            }
        }

        public async Task<Review?> GetReviewByIdAsync(int reviewId)
        {
            try
            {
                _logger.LogInformation("Fetching review with ReviewId: {ReviewId}", reviewId);
                var review = await _context.Reviews
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.IsActive);
                if (review == null)
                {
                    _logger.LogWarning("Review not found for ReviewId: {ReviewId}", reviewId);
                }
                return review;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching review with ReviewId: {ReviewId}", reviewId);
                throw;
            }
        }

        public async Task<bool> UpdateReviewAsync(Review review)
        {
            try
            {
                _logger.LogInformation("Updating review with ReviewId: {ReviewId}", review.ReviewId);
                var existingReview = await _context.Reviews.FindAsync(review.ReviewId);
                if (existingReview == null || !existingReview.IsActive)
                {
                    _logger.LogWarning("Review not found or inactive for ReviewId: {ReviewId}", review.ReviewId);
                    return false;
                }

                existingReview.Rating = review.Rating;
                existingReview.Comment = review.Comment;
                existingReview.ReviewType = review.ReviewType;
                existingReview.HotelId = review.HotelId;
                existingReview.UserId = review.UserId;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Review updated successfully: {ReviewId}", review.ReviewId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review with ReviewId: {ReviewId}", review.ReviewId);
                throw;
            }
        }

        public async Task<bool> SoftDeleteReviewAsync(int reviewId)
        {
            try
            {
                _logger.LogInformation("Soft deleting review with ReviewId: {ReviewId}", reviewId);
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null || !review.IsActive)
                {
                    _logger.LogWarning("Review not found or already deleted for ReviewId: {ReviewId}", reviewId);
                    return false;
                }

                review.IsActive = false;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Review soft deleted successfully: {ReviewId}", reviewId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting review with ReviewId: {ReviewId}", reviewId);
                throw;
            }
        }

        public async Task<double> CalculateHotelAverageRatingAsync(int hotelId)
        {
            try
            {
                _logger.LogInformation("Calculating average rating for HotelId: {HotelId}", hotelId);
                var reviews = await _context.Reviews
                    .Where(r => r.HotelId == hotelId && r.IsActive)
                    .ToListAsync();

                if (!reviews.Any())
                {
                    _logger.LogInformation("No active reviews found for HotelId: {HotelId}", hotelId);
                    return 0;
                }

                var averageRating = reviews.Average(r => r.Rating);
                _logger.LogInformation("Average rating for HotelId: {HotelId} is {AverageRating}", hotelId, averageRating);
                return averageRating;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating average rating for HotelId: {HotelId}", hotelId);
                throw;
            }
        }
    }
}