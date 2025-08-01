using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Models.Hotels;
using viaggia_server.Repositories;

namespace viaggia_server.Models.HotelDates
{
    public class HotelDate : ISoftDeletable
    {
        [Key]
        public int HotelDateId { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Available rooms is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Available rooms must be non-negative.")]
        public int AvailableRooms { get; set; }

        //[Required]
        //public int RoomTypeId { get; set; }

        //[ForeignKey("RoomTypeId")]
        //public virtual HotelRoomType RoomTypes { get; set; } = null!;

        [Required]
        public int HotelId { get; set; }

        [ForeignKey("HotelId")]
        public virtual Hotel Hotel { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<HotelRoomType> RoomTypes { get; set; }
    }
}