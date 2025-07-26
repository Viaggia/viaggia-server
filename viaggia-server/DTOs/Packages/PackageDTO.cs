using System.ComponentModel.DataAnnotations;
using viaggia_server.Models.Medias;

namespace viaggia_server.DTOs.Packages
{
    public class PackageDTO
    {
        public int PackageId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Destination { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public decimal BasePrice { get; set; }

        [Required]
        public int HotelId { get; set; }

        [Required]
        public string HotelName { get; set; } = null!;

        public bool IsActive { get; set; }

        public List<MediaDTO> Medias { get; set; } = new List<MediaDTO>();
        public List<PackageDateDTO> PackageDates { get; set; } = new List<PackageDateDTO>();
    }
}