using viaggia_server.DTOs.Commoditie;

namespace viaggia_server.DTOs.Hotel
{
    public class HotelSearchDTO
    {
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfPeople { get; set; }
        public CommoditieDTO? Commodities { get; set; }
    }
}