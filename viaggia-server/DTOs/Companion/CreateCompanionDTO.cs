using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs.Companion
{
    public class CreateAcompanhanteDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Document is required.")]
        [StringLength(20, ErrorMessage = "Document cannot exceed 20 characters.")]
        public string Document { get; set; } = null!;

        [Required(ErrorMessage = "Birth date is required.")]
        public DateTime BirthDate { get; set; }
    }
}
