using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Commodity
{
    public class CustomCommodityDTO
    {
        public int CustomCommodityId { get; set; }
        // Nome do hotel associado ao serviço
        [Required(ErrorMessage = "HotelName is required.")]
        public string HotelName { get; set; } = null!;

        // Nome do serviço personalizado (ex: "Lavanderia", "Translado", etc.)
        [Required(ErrorMessage = "ServiceName is required.")]
        [StringLength(100, ErrorMessage = "ServiceName cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        // Indica se o serviço é gratuito ou pago
        public bool IsPaid { get; set; }

        // Preço do serviço
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0.")]
        public decimal Price { get; set; }

        // Descrição do serviço (opcional)
        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        public string? Description { get; set; }

        // Indica se o serviço está ativo
        public bool IsActive { get; set; } = true;


        public int CommodityId { get; set; }
        public int HotelId { get; set; }

        public int CommoditieServicesId { get; set; }

    }
}