using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using viaggia_server.Repositories;
using ViaggiaServer.Models.Packages;

namespace viaggia_server.Models.Packages
{
    public class PackageDate : ISoftDeletable
    {
        [Key]
        public int PackageDateId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public int PackageId { get; set; }

        [ForeignKey("PackageId")]
        public virtual Package? Package { get; set; }
        public bool IsActive { get; set; } = true;

    }
}