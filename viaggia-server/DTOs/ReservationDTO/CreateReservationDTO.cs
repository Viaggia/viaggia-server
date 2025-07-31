using viaggia_server.Models.HotelRoomTypes;

namespace viaggia_server.DTOs.ReservationDTO
{
    public class CreateReservationDTO
    {
        public int UserId { get; set; }
        public string UserReservationName { get; set; }
        public int PackageId {  get; set; }
        public int RoomTypeId { get; set; }
        public int HotelId { get; set; }
        public int NumberGuests { get; set; }
        public DateTime CheckInDate { get; set; } = DateTime.UtcNow;
        public DateTime CheckOutDate { get; set; } = DateTime.UtcNow;
        public string TotalPrice { get; set; }
        public string Status { get; set; } = "Confirmed";
    }
}
