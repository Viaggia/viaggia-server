using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.CustomCommodities;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Reserves;
using viaggia_server.Models.Reviews;
using viaggia_server.Models.Users;
using viaggia_server.Repositories;

namespace viaggia_server.Models.Hotels
{
    public class Hotel : ISoftDeletable
    {
        [Key]
        public int HotelId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "CNPJ is required.")]
        public string Cnpj { get; set; } = null!;

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

        public double AverageRating { get; set; }
       
  
        [ForeignKey("UserId")]
        public int? UserId { get; set; } 
        public virtual User? User { get; set; } 

        // Relationships
        public virtual ICollection<HotelRoomType> RoomTypes { get; set; } = new List<HotelRoomType>();
        public virtual ICollection<Reserve> Reserves { get; set; } = new List<Reserve>();
        public virtual ICollection<Media> Medias { get; set; } = new List<Media>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Package> Packages { get; set; } = new List<Package>();
        public virtual ICollection<Commodity> Commodities { get; set; } = new List<Commodity>();
        public virtual ICollection<CustomCommodity> CustomCommodities { get; set; } = new List<CustomCommodity>();
        public List<Complaint> Complaints { get; set; } = new List<Complaint>();
    }
}