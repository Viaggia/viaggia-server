using viaggia_server.DTOs.Commodity;
using viaggia_server.DTOs.Packages;
using viaggia_server.DTOs.Reviews;
using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Hotel
{
    public class HotelDTO
    {
        [Required]
        public int HotelId { get; set; }
        public string Name { get; set; } = null!;
        public string Cnpj { get; set; } = null!;
        public string Street { get; set; } = null!;
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public string ZipCode { get; set; } = null!;
        public string? Description { get; set; }
        public int StarRating { get; set; }
        public string? CheckInTime { get; set; }
        public string? CheckOutTime { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public bool IsActive { get; set; }
        public double AverageRating { get; set; }
        public List<HotelRoomTypeDTO> RoomTypes { get; set; } = new List<HotelRoomTypeDTO>();
        public List<MediaDTO> Medias { get; set; } = new List<MediaDTO>();
        public List<ReviewDTO> Reviews { get; set; } = new List<ReviewDTO>();
        public List<PackageDTO> Packages { get; set; } = new List<PackageDTO>();
        public List<CommodityDTO> Commodities { get; set; } = new List<CommodityDTO>();
        public List<CustomCommodityDTO> CustomCommodities { get; set; } = new List<CustomCommodityDTO>();
    }
}