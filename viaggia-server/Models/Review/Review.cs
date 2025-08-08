using System.ComponentModel.DataAnnotations;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Users;
using viaggia_server.Repositories;

namespace viaggia_server.Models.Reviews
{
    public class Review : ISoftDeletable
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        public int UserId { get; set; }

        public virtual User User { get; set; } = null!;

        [Required]
        [StringLength(20, ErrorMessage = "Review type cannot exceed 20 characters.")]
        public string ReviewType { get; set; } = null!; // "Hotel" ou "Agency"

        public int? HotelId { get; set; }

        public virtual Hotel? Hotel { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string? Comment { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}