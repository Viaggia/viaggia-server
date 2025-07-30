namespace viaggia_server.DTOs.ReservationDTO
{
    public class CreateReservation
    {
        public string UserId { get; set; } = null!;
        public string ReservationId { get; set; } = null!;
        public string HotelId { get; set; } = null!;
        public DateTime CheckInDate { get; set; } = DateTime.UtcNow;
        public DateTime CheckOutDate { get; set; } = DateTime.UtcNow;
        public decimal TotalPrice { get; set; } = 0.0m;
    }
}
