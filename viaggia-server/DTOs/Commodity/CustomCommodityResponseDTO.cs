namespace viaggia_server.DTOs.Commodity
{
    public class CustomCommodityResponseDTO
    {
        public int CustomCommodityId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } 
        public bool IsPaid { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}

