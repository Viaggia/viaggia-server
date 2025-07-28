using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Packages
{
    public class PackageCreateDTO
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Destination { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public decimal BasePrice { get; set; }

        [Required]
        public int HotelId { get; set; }

        public bool IsActive { get; set; } = true;

        public List<PackageDateDTO> PackageDates { get; set; } = new List<PackageDateDTO>();
        public List<IFormFile> MediaFiles { get; set; } = new List<IFormFile>();
    }
}