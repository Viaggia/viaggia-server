using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Packages
{
    public class PackageDateDTO
    {
        public int PackageDateId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}