using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Packages;
using viaggia_server.Repositories;

namespace viaggia_server.Models.Medias
{
    public class Media : ISoftDeletable
    {
        [Key]
        public int MediaId { get; set; }

        [Required(ErrorMessage = "Media URL is required.")]
        public string MediaUrl { get; set; } = null!;

        [Required(ErrorMessage = "Media type is required.")]
        [StringLength(20, ErrorMessage = "Media type cannot exceed 20 characters.")]
        public string MediaType { get; set; } = null!; // Ex.: "image", "video"

        public int? PackageId { get; set; }

        [ForeignKey("PackageId")]
        public virtual Package? Package { get; set; }

        public int? HotelId { get; set; }

        [ForeignKey("HotelId")]
        public virtual Hotel? Hotel { get; set; }

        public bool IsActive { get; set; } = true;
    }
}