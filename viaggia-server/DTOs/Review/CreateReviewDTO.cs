using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Reviews
{
    public class CreateReviewDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Review type cannot exceed 20 characters.")]
        public string ReviewType { get; set; } = null!;

        public int? HotelId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string? Comment { get; set; }
    }
}