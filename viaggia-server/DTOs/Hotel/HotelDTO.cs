using System.ComponentModel.DataAnnotations;
using viaggia_server.DTOs.Address;
using viaggia_server.DTOs.Commoditie;
using viaggia_server.DTOs.Commodity;
using viaggia_server.DTOs.Packages;
using viaggia_server.DTOs.Reviews;

namespace viaggia_server.DTOs.Hotels
{
    public class HotelDTO
    {
        [Required]
        public int HotelId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "CNPJ is required.")]
        public string Cnpj { get; set; } = null!; // CNPJ for service providers

      

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Range(1, 5, ErrorMessage = "Star rating must be between 1 and 5.")]
        public int StarRating { get; set; }

        [StringLength(10, ErrorMessage = "Check-in time cannot exceed 10 characters.")]
        public string? CheckInTime { get; set; }

        [StringLength(10, ErrorMessage = "Check-out time cannot exceed 10 characters.")]
        public string? CheckOutTime { get; set; }

        [StringLength(20, ErrorMessage = "Contact phone cannot exceed 20 characters.")]
        public string? ContactPhone { get; set; }

        [StringLength(100, ErrorMessage = "Contact email cannot exceed 100 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? ContactEmail { get; set; }

        public bool IsActive { get; set; } = true;

        public List<HotelRoomTypeDTO> RoomTypes { get; set; } = new List<HotelRoomTypeDTO>();
        public List<HotelDateDTO> HotelDates { get; set; } = new List<HotelDateDTO>();
        public List<MediaDTO> Medias { get; set; } = new List<MediaDTO>();
        public List<CreateAddressDTO> Addresses { get; set; } = new List<CreateAddressDTO>();
        public List<ReviewDTO> Reviews { get; set; } = new List<ReviewDTO>(); 
        public List<PackageDTO> Packages { get; set; } = new List<PackageDTO>();
        public List<CommoditieDTO> Commodities { get; set; } = new List<CommoditieDTO>();
        public List<CommoditieServicesDTO> CommoditieServices { get; set; } = new List<CommoditieServicesDTO>();
        public double AverageRating { get; set; } // Média das avaliações

    }
}