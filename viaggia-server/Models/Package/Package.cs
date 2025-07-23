using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Reservations;
using viaggia_server.Repositories;


namespace ViaggiaServer.Models.Packages
{
    public class Package : ISoftDeletable
    {
        [Key]
        public int PackageId { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Destination { get; set; }

        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Required]
        public decimal BasePrice { get; set; }

        public bool IsActive { get; set; }

        // Relacionamentos
        public virtual ICollection<Media> Medias { get; set; } = new List<Media>(); // Alterado de PackageMedia
        public virtual ICollection<PackageDate> PackageDates { get; set; } = new List<PackageDate>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
