namespace viaggia_server.DTOs.Complaint
{
    public class ComplaintDTO
    {
        public int ComplaintId { get; set; }
        public int UserId { get; set; }
        public int HotelId { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
