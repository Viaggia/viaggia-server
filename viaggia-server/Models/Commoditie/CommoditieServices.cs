using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using viaggia_server.Repositories;

namespace viaggia_server.Models.Commodities
{
    public class CommoditieServices : ISoftDeletable
    {
            [Key]
            public int CommoditieServicesId { get; set; }

           [Required(ErrorMessage = "Service name is required")]
           [StringLength(100, ErrorMessage = "The service name cannot exceed 100 characters")]
      
           public string Name { get; set; } = null!; // Nome do serviço extra (ex: "Serviço de quarto 24h")

            public bool IsPaid { get; set; }

           [StringLength(250, ErrorMessage = "The description cannot exceed 250 characters")]
            public string? Description { get; set; }

            public bool IsActive { get; set; } = true;

            public int CommoditieId { get; set; }

            [ForeignKey("CommoditieId")]
            public Commoditie Commoditie { get; set; } = null!;
        
    }
}

