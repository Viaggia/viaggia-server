namespace viaggia_server.DTOs.Commoditie
{
    public class CommoditieServicesResponseDTO
    {
        public int CommoditieServicesId { get; set; }
        public string HotelName { get; set; } = string.Empty; 
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } 
        public bool IsPaid { get; set; }
        public bool IsActive { get; set; }

   
    }
}


