using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Packages
{
    public class PackageUpdateDTO
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Destination { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "BasePrice must be greater than 0.")]
        public decimal BasePrice { get; set; }

        [Required]
        public string HotelName { get; set; } = null!;

        public bool IsActive { get; set; }

        [RegularExpression(@"^\d{2}/\d{2}/\d{4}$", ErrorMessage = "StartDate must be in DD/MM/YYYY format.")]
        public string? StartDate { get; set; }

        [RegularExpression(@"^\d{2}/\d{2}/\d{4}$", ErrorMessage = "EndDate must be in DD/MM/YYYY format.")]
        public string? EndDate { get; set; }

        public List<int> MediaIdsToDelete { get; set; } = new List<int>();

        public List<IFormFile> NewMediaFiles { get; set; } = new List<IFormFile>();
    }
}