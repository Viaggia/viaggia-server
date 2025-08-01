using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Commoditie
{
    public class CreateCommoditieServicesDTO
    {
        [Required(ErrorMessage = "Service name is required.")]
        [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        public bool IsPaid { get; set; }

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
