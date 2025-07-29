using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using viaggia_server.DTOs.Commoditie;
using viaggia_server.DTOs.Commodity;
using viaggia_server.DTOs.Hotels;
using viaggia_server.DTOs.Packages;
using viaggia_server.DTOs.Reviews;

namespace viaggia_server.DTOs.Hotels
{
   
    public class CreateHotelDTO
    {
       
            [Required(ErrorMessage = "Name is required.")]
            [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
            
            public string Name { get; set; } = null!;

            [Required(ErrorMessage = "Street is required.")]
            [StringLength(100, ErrorMessage = "Street cannot exceed 100 characters.")]
           
            public string Street { get; set; } = null!;

            [Required(ErrorMessage = "City is required.")]
            [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
           
            public string City { get; set; } = null!;

            [Required(ErrorMessage = "State is required.")]
            [StringLength(50, ErrorMessage = "State cannot exceed 50 characters.")]
            
            public string State { get; set; } = null!;

            [Required(ErrorMessage = "Zip code is required.")]
            [StringLength(20, ErrorMessage = "Zip code cannot exceed 20 characters.")]
            
            public string ZipCode { get; set; } = null!;

            [Required(ErrorMessage = "CNPJ is required.")]
            [StringLength(14, ErrorMessage = "CNPJ must be 14 characters.")]
            
            public string Cnpj { get; set; } = null!;

            [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
            
            public string? Description { get; set; }

            [Required]
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

           
            public string? RoomTypesJson { get; set; }

           
            public string? HotelDatesJson { get; set; }
 
            public string? CommoditieJson { get; set; }





        public List<IFormFile> MediaFiles { get; set; } = new List<IFormFile>();
        public List<HotelDateDTO> HotelDates { get; set; } = new List<HotelDateDTO>();
        public List<HotelRoomTypeDTO> RoomTypes { get; set; } = new List<HotelRoomTypeDTO>();

        public List<MediaDTO> Medias { get; set; } = new List<MediaDTO>();

        public List<PackageDTO> Packages { get; set; } = new List<PackageDTO>();
        public List<ReviewDTO> Reviews { get; set; } = new List<ReviewDTO>();
        public List<CommoditieDTO> Commodities { get; set; } = new List<CommoditieDTO>();
        public List<CommoditieServicesDTO> CommoditieServices { get; set; } = new List<CommoditieServicesDTO>();
        public double AverageRating { get; set; } // Média das avaliações


    }
}
