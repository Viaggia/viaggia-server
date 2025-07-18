using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using viaggia_server.Models.Destination;

namespace viaggia_server.Models.Package
{
    public class Package
    {
        [Key]
        public int PackageId { get; set; }
        [Required] 
        public string? Name { get; set; }
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Required]
        public decimal BasePrice { get; set; }
        public string? Image { get; set; }
        public bool Is_Closed { get; set; }
        public int DestinationId { get; set; } // Chave estrangeira para o Destino
        public Destination? Destination { get; set; } // Relacionamento: 1 Pacote → 1 Destino

        public ICollection<PackageDate>? PackageDates { get; set; } // Relacionamento: 1 Pacote → N Pacote_Data

        
    }
}
