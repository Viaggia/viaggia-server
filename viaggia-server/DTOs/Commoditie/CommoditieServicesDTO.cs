using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace viaggia_server.DTOs.Commoditie
{

    public class CommoditieServicesDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        public bool IsPaid { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
        public int CommoditieServicesId { get; internal set; }
        public bool IsActive { get; internal set; }
        public int CommoditieId { get; internal set; }
    }
}
