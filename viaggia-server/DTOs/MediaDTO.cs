using System.ComponentModel.DataAnnotations;

namespace viaggia_server.DTOs
{
    public class MediaDTO
    {
        public int MediaId { get; set; }

        [Required]
        public string MediaUrl { get; set; } = null!;

        [Required]
        public string MediaType { get; set; } = null!;
    }
}