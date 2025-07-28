using System.ComponentModel.DataAnnotations;
using viaggia_server.DTOs.Commodities;
using viaggia_server.DTOs.Hotels;

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

        public List<HotelRoomTypeDTO> RoomTypes { get; set; } = new List<HotelRoomTypeDTO>();
        public List<HotelDateDTO> HotelDates { get; set; } = new List<HotelDateDTO>();
        public List<IFormFile> MediaFiles { get; set; } = new List<IFormFile>();

        // ✅ Associação 1:1 com Commoditie
        public CreateCommoditieDTO Commoditie { get; set; } = new CreateCommoditieDTO();
    }
}
