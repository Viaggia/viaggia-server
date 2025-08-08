namespace viaggia_server.DTOs.Complaint
{
    public class CreateComplaintDTO
    {
        public int UserId { get; set; }
        public int HotelId { get; set; }
        public string Comment { get; set; }
    }
}
