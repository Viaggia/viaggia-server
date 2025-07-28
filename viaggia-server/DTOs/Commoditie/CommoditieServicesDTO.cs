using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Commodities
{
    public class CommoditieServicesDTO
    {
        // Nome do serviço personalizado (ex: "Lavanderia", "Translado", etc.)
        [Required(ErrorMessage = "ServiceName is required.")]
        [StringLength(100, ErrorMessage = "ServiceName cannot exceed 100 characters.")]
        public string ServiceName { get; set; } = string.Empty;

        // Indica se o serviço é gratuito ou pago
        public bool IsFree { get; set; }

        // Indica se o serviço está ativo
        public bool IsActive { get; set; } = true;
    }
}

