namespace viaggia_server.DTOs.Reserves
{
    public class ReserveCreateDTO
    {
        public int UserId { get; set; }
        public int? PackageId { get; set; }
        public int RoomTypeId { get; set; }
        public int HotelId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int TotalPrice { get; set; }
        public int NumberOfGuests { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}