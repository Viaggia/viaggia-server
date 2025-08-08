using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Reserves;
using viaggia_server.Models.Users;
using viaggia_server.Repositories;

namespace viaggia_server.Models.Packages
{
    public class Package : ISoftDeletable
    {
        [Key]
        public int PackageId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Destination { get; set; } = null!;

        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Required]
        public decimal BasePrice { get; set; }

        [Required]
        public int HotelId { get; set; } // New foreign key

        [ForeignKey("HotelId")]
        public virtual Hotel Hotel { get; set; } = null!;

        public int UserId { get; set; }
        // Foreign key for User
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public bool IsActive { get; set; }

        // Relationships
        public virtual ICollection<Media> Medias { get; set; } = new List<Media>();
        public virtual ICollection<PackageDate> PackageDates { get; set; } = new List<PackageDate>();
        public virtual ICollection<Reserve> Reserves { get; set; } = new List<Reserve>();
    }
}