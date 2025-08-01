namespace viaggia_server.DTOs.Commoditie
{
    public class CommoditieServicesResponseDTO
    {
        public int CommoditieServicesId { get; set; }
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; } // Nullable para permitir serviços sem descrição
        public bool IsPaid { get; set; }
        public bool IsActive { get; set; }

        public string HotelName { get; set; } = string.Empty; // Apenas isso a mais
    }
}


