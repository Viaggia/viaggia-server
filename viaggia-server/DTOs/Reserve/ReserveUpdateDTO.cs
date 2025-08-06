using System.ComponentModel.DataAnnotations;
using viaggia_server.DTOs.Reserve;

namespace viaggia_server.DTOs.Reserves
{
    public class ReserveUpdateDTO
    {
        public int ReserveId { get; set; }
        public int UserId { get; set; }
        public int? PackageId { get; set; }
        public int? HotelId { get; set; }

        [Required(ErrorMessage = "Check-in date is required.")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-out date is required.")]
        public DateTime CheckOutDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total price cannot be negative.")]
        public decimal TotalPrice { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Number of guests must be at least 1.")]
        public int NumberOfGuests { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "At least one room must be reserved.")]
        public List<ReserveRoomUpdateDTO> ReserveRooms { get; set; } = new List<ReserveRoomUpdateDTO>();
    }
}
