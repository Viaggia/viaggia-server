using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Hotels
{
    public class HotelDateDTO
    {
        public int HotelDateId { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Available rooms is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Available rooms must be non-negative.")]
        public int AvailableRooms { get; set; }

        public bool IsActive { get; set; } = true;
    }
}