using viaggia_server.DTOs.Reserve;

namespace viaggia_server.DTOs.Reserves
{
    public class ReserveDTO
    {
        public int ReserveId { get; set; }
        public int UserId { get; set; }
        public int? PackageId { get; set; }
        public int? HotelId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public int NumberOfPeople { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<ReserveRoomDTO> ReserveRooms { get; set; } = new List<ReserveRoomDTO>();
    }
}
