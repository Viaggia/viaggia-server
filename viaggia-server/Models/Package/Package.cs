using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace viaggia_server.Models.Package
{
    public class Package
    {
        [Key]
        public int PackageId { get; set; }
        [Required] 
        public string Name { get; set; }

        public string? Local { get; set; }
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Required]
        public decimal BasePrice { get; set; }
        public string? Image { get; set; }
        public bool IsClosed { get; set; }
       
        public ICollection<PackageDate>? PackageDates { get; set; } // Relacionamento: 1 Pacote → N Pacote_Data

        
    }
}
