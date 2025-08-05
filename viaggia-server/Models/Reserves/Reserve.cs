using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Users;
using viaggia_server.Repositories;

namespace viaggia_server.Models.Reserves
{
    public class Reserve : ISoftDeletable
    {
        [Key]
        public int ReserveId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public int? HotelId { get; set; }

        [ForeignKey("HotelId")]
        public virtual Hotel? Hotel { get; set; }

        public int? PackageId { get; set; }

        [ForeignKey("PackageId")]
        public virtual Package? Package { get; set; }

        public int? RoomTypeId { get; set; }

        [ForeignKey("RoomTypeId")]
        public virtual HotelRoomType? HotelRoomType { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Number of guests must be at least 1.")]
        public int NumberOfGuests { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }
        public decimal TotalDiscount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "pendente";
        public int NumberOfRooms { get; set; }
        public bool IsActive { get; set; } = true;
    }
}