using System.ComponentModel.DataAnnotations;
using viaggia_server.Models.Users;

namespace viaggia_server.Models.PasswordResetToken
{
    public class PasswordResetToken
    {
        [Key]
        public string Token { get; set; } = null!;
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; }
        public User User { get; set; } = null!;
    }
}
