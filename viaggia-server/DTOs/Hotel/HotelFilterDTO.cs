namespace viaggia_server.DTOs.HotelFilterDTO
{
    public class HotelFilterDTO
    {
        public List<string> Commodities { get; set; } = new List<string>(); 
        public List<string> CommoditieServices { get; set; } = new List<string>(); 
        public List<string> RoomTypes { get; set; } = new List<string>(); 
    }
}
