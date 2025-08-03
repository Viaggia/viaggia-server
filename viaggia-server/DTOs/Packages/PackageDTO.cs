
namespace viaggia_server.DTOs.Packages
{
    public class PackageDTO
        {
            public int PackageId { get; set; }
            public string Name { get; set; } = null!;
            public string Destination { get; set; } = null!;
            public string? Description { get; set; }
            public decimal BasePrice { get; set; }
            public int HotelId { get; set; }
            public string HotelName { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public List<MediaDTO> Medias { get; set; } = new List<MediaDTO>();
            public List<PackageDateDTO> PackageDates { get; set; } = new List<PackageDateDTO>();
        }
    }