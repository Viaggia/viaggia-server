namespace viaggia_server.DTOs.Reserve
{
    public class ReserveDTO
    {
        public int ReserveId { get; set; }
        public int UserId { get; set; }
        public int? HotelId { get; set; }
        public int? RoomTypeId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int? NumberOfRooms { get; set; }
        public int? NumberOfGuests { get; set; }
        public decimal?  TotalPrice { get; set; }
        public string Status { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
