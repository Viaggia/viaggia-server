namespace viaggia_server.DTOs.Reservation
{
    public class ReservationDTO
    {
        public int ReservationId { get; set; }
        public int UserId { get; set; }
        public int? PackageId { get; set; }
        public int? HotelId { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsActive { get; set; }
    }
}