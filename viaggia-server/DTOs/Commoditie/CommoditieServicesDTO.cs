using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Commodities
{
    public class CommoditieServicesDTO
    {
        // Nome do serviço personalizado (ex: "Lavanderia", "Translado", etc.)
        [Required(ErrorMessage = "ServiceName is required.")]
        [StringLength(100, ErrorMessage = "ServiceName cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        // Indica se o serviço é gratuito ou pago
        public bool IsPaid { get; set; }

        // Descrição do serviço (opcional)
        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        public string? Description { get; set; }

        // Indica se o serviço está ativo
        public bool IsActive { get; set; } = true;
    }
}

