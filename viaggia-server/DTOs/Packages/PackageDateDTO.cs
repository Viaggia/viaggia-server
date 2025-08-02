using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Packages
{
    public class PackageDateDTO
    {
        public int PackageDateId { get; set; }


        [Required]
        [RegularExpression(@"^\d{2}/\d{2}/\d{4}$", ErrorMessage = "StartDate must be in DD/MM/YYYY format.")]
        public string StartDate { get; set; } = null!;

        [Required]
        [RegularExpression(@"^\d{2}/\d{2}/\d{4}$", ErrorMessage = "EndDate must be in DD/MM/YYYY format.")]
        public string EndDate { get; set; } = null!;
    }
}