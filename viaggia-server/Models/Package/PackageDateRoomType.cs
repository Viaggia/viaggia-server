using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace viaggia_server.Models.Package
{
    public class PackageDateRoomType
    {
        [Key]
        public int PackageDateRoomTypeId { get; set; }

        [Required]
        public int Vacancy { get; set; }

        public int PackageDateId { get; set; } // Chave estrangeira para o Pacote_Data

        [ForeignKey("PackageDateId")]
        public PackageDate? PackageDate { get; set; } // Relacionamento: 1 Pacote_Data_TipoQuarto → 1 Pacote_Data

        public int RoomTypeId { get; set; } // Chave estrangeira para o TipoQuarto

        [ForeignKey("RoomTypeId")]
        public RoomType? RoomType { get; set; } // Relacionamento: 1 Pacote_Data_TipoQuarto → 1 TipoQuarto
    }
}
