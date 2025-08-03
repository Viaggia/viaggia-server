using viaggia_server.Models.RoomTypeEnums;

namespace viaggia_server.DTOs.Hotels
{
    public class HotelRoomTypeDTO
    {
        public int RoomTypeId { get; set; }
        public RoomTypeEnum Name { get; set; } 
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public string? BedType { get; set; }
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public bool IsActive { get; set; }
    }
}