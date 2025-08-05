﻿using viaggia_server.Models.Reviews;

namespace viaggia_server.Repositories
{
    public interface IReviewRepository
    {
        Task<Review> CreateReviewAsync(Review review);
        Task<IEnumerable<Review>> GetReviewsByHotelIdAsync(int hotelId);
        Task<Review?> GetReviewByIdAsync(int reviewId);
        Task<bool> UpdateReviewAsync(Review review);
        Task<bool> SoftDeleteReviewAsync(int reviewId);
        Task<double> CalculateHotelAverageRatingAsync(int hotelId);
    }
}