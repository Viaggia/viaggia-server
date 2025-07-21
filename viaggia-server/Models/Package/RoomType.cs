using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Column(TypeName = "decimal(10,2)")]
        [Required]
        public decimal ExtraValue { get; set; }

        public ICollection<PackageDateRoomType>? PackageDateRooms { get; set; } = new List<PackageDateRoomType>();// Relacionamento: 1 TipoQuarto → N Pacote_Data_TipoQuarto
    }
}
