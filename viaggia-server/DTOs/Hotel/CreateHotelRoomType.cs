using System.ComponentModel.DataAnnotations;
using viaggia_server.Models.RoomTypeEnums;

namespace viaggia_server.DTOs.Hotel
{
    public class CreateHotelRoomTypeDTO
    {
        [Required(ErrorMessage = "Room type is required.")]
        public RoomTypeEnum Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, 10, ErrorMessage = "Capacity must be between 1 and 10.")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Bed type is required.")]
        [StringLength(50, ErrorMessage = "Bed type cannot exceed 50 characters.")]
        public string BedType { get; set; } = null!;

        [Required(ErrorMessage = "Total rooms is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Total rooms must be positive.")]
        public int TotalRooms { get; set; }
    }
}