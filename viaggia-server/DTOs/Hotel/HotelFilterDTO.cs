 namespace viaggia_server.DTOs.Hotel
    {
        public class HotelFilterDTO
        {
            public List<string> Commodity { get; set; } = new List<string>();
            public List<string> CustomCommodity { get; set; } = new List<string>();
            public List<string> RoomTypes { get; set; } = new List<string>();
            public decimal? MinPrice { get; set; } // Minimum total price (room + paid commodities/services)
            public decimal? MaxPrice { get; set; } // Maximum total price (room + paid commodities/services)
            public int? MinCapacity { get; set; } // Minimum room capacity
        }
    }