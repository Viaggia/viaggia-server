using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using viaggia_server.Models.Users;

namespace viaggia_server.Models.Users
{
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Token deve ter exatamente 6 dígitos")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Token deve conter apenas números")]
        public string Token { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime? UsedAt { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        // Token é válido se não expirou e não foi usado
        [NotMapped]
        public bool IsValid => DateTime.UtcNow < ExpiresAt && !IsUsed;
    }
}
