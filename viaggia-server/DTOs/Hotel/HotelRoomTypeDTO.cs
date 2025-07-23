using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Hotels
{
    public class HotelRoomTypeDTO
    {
        public int RoomTypeId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, 10, ErrorMessage = "Capacity must be between 1 and 10.")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Bed type is required.")]
        [StringLength(50, ErrorMessage = "Bed type cannot exceed 50 characters.")]
        public string BedType { get; set; } = null!;

        public bool IsActive { get; set; }
    }
}