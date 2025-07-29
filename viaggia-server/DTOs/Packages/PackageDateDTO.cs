using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Packages
{
    public class PackageDateDTO
    {
        public int PackageDateId { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        public DateTime EndDate { get; set; }
    }
}