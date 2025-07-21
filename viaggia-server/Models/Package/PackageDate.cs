using System.ComponentModel.DataAnnotations;

namespace viaggia_server.Models.Package
{
    public class PackageDate
    {
        [Key]
        public int PackageDateId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public int PackageId { get; set; } // Chave estrangeira para o Pacote
        public Package? Package { get; set; } // Relacionamento: 1 Pacote_Data → 1 Pacote

        public ICollection<PackageDateRoomType>? PackageDateRoomTypes { get; set; } // Relacionamento: 1 Pacote_Data → N Pacote_Data_TipoQuarto

    }
}
