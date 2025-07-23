using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Payments;
using viaggia_server.Models.Users;
using viaggia_server.Repositories;
using ViaggiaServer.Models.Packages;

namespace viaggia_server.Models.Reservations
{
    public class Reservation : ISoftDeletable
    {
        [Key]
        public int ReservationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public int? PackageId { get; set; }

        [ForeignKey("PackageId")]
        public virtual Package? Package { get; set; }

        public int? RoomTypeId { get; set; }

        [ForeignKey("RoomTypeId")]
        public virtual HotelRoomType? HotelRoomType { get; set; }

        public int? HotelId { get; set; }

        [ForeignKey("HotelId")]
        public virtual Hotel? Hotel { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Total price is required.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Number of guests is required.")]
        [Range(1, 10, ErrorMessage = "Number of guests must be between 1 and 10.")]
        public int NumberOfGuests { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        public string Status { get; set; } = null!; // Ex.: "Confirmed", "Cancelled", "Pending"

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}