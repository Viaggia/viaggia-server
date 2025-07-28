using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using viaggia_server.Models.Reservations;
using viaggia_server.Repositories;

namespace viaggia_server.Models.Companions
{
    public class Companion : ISoftDeletable
    {
        [Key]
        public int CompanionId { get; set; }

        [Required(ErrorMessage = "Reservation ID is required.")]
        public int ReservationId { get; set; }

        [ForeignKey("ReservationId")]
        public virtual Reservation Reservation { get; set; } = null!;

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Document is required.")]
        [StringLength(20, ErrorMessage = "Document cannot exceed 20 characters.")]
        public string CPF { get; set; } = null!;

        [Required(ErrorMessage = "Birth date is required.")]
        public DateTime BirthDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}