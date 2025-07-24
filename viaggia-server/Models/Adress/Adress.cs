using System.ComponentModel.DataAnnotations;
using viaggia_server.Repositories;

namespace viaggia_server.Models.Addresses
{
    public class Address : ISoftDeletable
    {
        [Key]
        public int AddressId { get; set; }

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

        public bool IsActive { get; set; } = true;
    }
}