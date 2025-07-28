using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using viaggia_server.Models.Addresses;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.HotelDates;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Reservations;
using viaggia_server.Models.Reviews;
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
        public string Cnpj { get; set; } = null!; // CNPJ for service providers

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

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

        [Required(ErrorMessage = "Address is required.")]
        public int AddressId { get; set; }

        [ForeignKey("AddressId")]
        public Address? Address { get; set; }

        [ForeignKey("CommoditieId")]
        public Commoditie? Commoditie { get; set; } = null!;

        // Relationships
        public virtual ICollection<HotelRoomType> RoomTypes { get; set; } = new List<HotelRoomType>();
        public virtual ICollection<HotelDate> HotelDates { get; set; } = new List<HotelDate>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<Media> Medias { get; set; } = new List<Media>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Package> Packages { get; set; } = new List<Package>(); // New collection
        public virtual ICollection<Commoditie> Commodities { get; set; } = new List<Commoditie>();
        public virtual ICollection<CommoditieServices> CommoditieServices { get; set; } = new List<CommoditieServices>();

        public double AverageRating { get; set; } // Média das avaliações

        // public virtual ICollection<Address> Addresses { get; set; } = new List<Address>(); // New collection for addresses - Necessariamente o mesmo hotel com o mesmo nome na barra da tijuca não é o de copacabana
    }
}