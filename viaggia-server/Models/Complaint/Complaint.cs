using viaggia_server.Models.Hotels;
using viaggia_server.Models.Users;

namespace viaggia_server.Models
{
    public class Complaint
    {
        public int ComplaintId { get; set; }
        public int UserId { get; set; }
        public int HotelId { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Hotel Hotel { get; set; }
    }
}