using viaggia_server.Models;

namespace viaggia_server.Repositories
{
    public interface IComplaintRepository
    {
        Task<Complaint> CreateComplaintAsync(Complaint complaint);
        Task<List<Complaint>> GetComplaintsByHotelIdAsync(int hotelId);
    }
}
