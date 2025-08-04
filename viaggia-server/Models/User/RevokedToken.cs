using System.ComponentModel.DataAnnotations;

namespace viaggia_server.Models.Users
{
    public class RevokedToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; } = null!; 
        public DateTime RevokedAt { get; set; }
        public DateTime? ExpiryDate { get; set; } 
    }
}
