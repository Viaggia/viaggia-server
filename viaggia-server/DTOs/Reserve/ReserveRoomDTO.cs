namespace viaggia_server.DTOs.Reserve
{
    public class ReserveRoomDTO
    {
        public int RoomTypeId { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
