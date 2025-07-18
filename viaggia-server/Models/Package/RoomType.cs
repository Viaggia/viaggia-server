using System.ComponentModel.DataAnnotations;

namespace viaggia_server.Models.Package
{
    public class RoomType
    {
        [Key]
        public int RoomTypeId { get; set; }

        [Required]
        [StringLength(150)]
        public string? Name { get; set; }
        public int Capacity { get; set; }
        public decimal ExtraValue { get; set; }

        public ICollection<PackageDateRoomType>? PackageDateRooms { get; set; } // Relacionamento: 1 TipoQuarto → N Pacote_Data_TipoQuarto
    }
}
