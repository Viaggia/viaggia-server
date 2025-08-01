using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace viaggia_server.DTOs.Commoditie
{
    public class CommoditieServicesDTO
    {
        // Nome do serviço personalizado (ex: "Lavanderia", "Translado", etc.)
        [Required(ErrorMessage = "ServiceName is required.")]
        [StringLength(100, ErrorMessage = "ServiceName cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        // Nome do hotel associado ao serviço
        [Required(ErrorMessage = "HotelName is required.")]
        public string HotelName { get; set; } = null!;

        // Indica se o serviço é gratuito ou pago
        public bool IsPaid { get; set; }

        // Descrição do serviço (opcional)
        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        public string? Description { get; set; }

        // Indica se o serviço está ativo
        public bool IsActive { get; set; } = true;

        public int CommoditieId { get; set; }

        public int HotelId { get; set; }

    }
}
