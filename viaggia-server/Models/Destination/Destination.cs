using System.ComponentModel.DataAnnotations;
using viaggia_server.Models.Package;

namespace viaggia_server.Models.Destination
{
    public class Destination
    {
        [Key]
        public int DestinationId { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        public string? City { get; set; }
        public string? State { get; set; }

        public string? Country { get; set; }

        public ICollection<Package>? Packages { get; set; } // Relacionamento: 1 Destino → N Pacotes
    }
}
