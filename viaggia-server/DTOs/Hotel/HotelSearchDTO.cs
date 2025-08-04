using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Hotel
{
    public class HotelSearchDTO
    {
        [Required(ErrorMessage = "Destination city is required.")]
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
        public string City { get; set; } = null!;

        [Required(ErrorMessage = "Check-in date is required.")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-out date is required.")]
        public DateTime CheckOutDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Number of people must be at least 1.")]
        public int NumberOfPeople { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Number of rooms must be at least 1.")]
        public int NumberOfRooms { get; set; }
    }
}