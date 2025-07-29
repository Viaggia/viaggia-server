namespace viaggia_server.DTOs.Reservation
{
    public class ReservationUpdateDTO
    {
        public int ReservationId { get; set; }
        public int UserId { get; set; }
        public int? PackageId { get; set; }
        public int? RoomTypeId { get; set; }
        public int? HotelId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public int NumberOfGuests { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}