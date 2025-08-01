using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.RoomTypeEnums;
using viaggia_server.Repositories;

namespace viaggia_server.Models.HotelRoomTypes
{
    public class HotelRoomType : ISoftDeletable
    {
        [Key]
        public int RoomTypeId { get; set; }

        [Required(ErrorMessage = "Room type name is required.")]
        public RoomTypeEnum Name { get; set; } 

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1.")]
        public int Capacity { get; set; }

        [StringLength(50, ErrorMessage = "Bed type cannot exceed 50 characters.")]
        public string? BedType { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Total rooms must be at least 1.")]
        public int TotalRooms { get; set; }

        public int AvailableRooms { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public int HotelId { get; set; }

        [ForeignKey("HotelId")]
        public virtual Hotel Hotel { get; set; } = null!;

        public int HotelDateId { get; set; }
        [ForeignKey("HotelDateId")]
        public virtual HotelDate HotelDate { get; set; }
    }
}