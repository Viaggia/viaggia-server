using viaggia_server.DTOs;
using viaggia_server.DTOs.Complaint;
using viaggia_server.DTOs.Hotel;
using viaggia_server.DTOs.Packages;
using viaggia_server.DTOs.Reserve;
using viaggia_server.DTOs.Reviews;
using viaggia_server.Models.Hotels;


namespace viaggia_server.Services.HotelServices
{
    public interface IHotelServices
    {

        //CRUD
        Task<ApiResponse<List<HotelDTO>>> GetAllHotelAsync();
        Task<ApiResponse<HotelDTO>> GetHotelByIdAsync(int id);
        Task<ApiResponse<Hotel>> CreateHotelAsync(CreateHotelDTO createHotelDto, List<CreateHotelRoomTypeDTO> roomTypes, int userId);
        Task<ApiResponse<HotelDTO>> UpdateHotelAsync(UpdateHotelDto updateHotelDto, List<CreateHotelRoomTypeDTO>? roomTypes);
        Task<bool> SoftDeleteHotelAsync(int id);


        //filtros e buscas
        Task<ApiResponse<List<HotelDTO>>> GetHotelsByUserIdAsync(int userId);
        Task<ApiResponse<List<ReserveDTO>>> GetReservationsByHotelIdAsync(int hotelId);
        Task<ApiResponse<double>> GetHotelAverageRatingAsync(int hotelId);
        Task<ApiResponse<IEnumerable<PackageDTO>>> GetPackagesByHotelIdAsync(int hotelId);
        Task<ApiResponse<List<HotelDTO>>> FilterHotelsAsync(HotelFilterDTO filter);
        Task<ApiResponse<List<HotelDTO>>> SearchHotelsByDestinationAsync(HotelSearchDTO searchDto);

        Task<ApiResponse<List<HotelRoomTypeDTO>>> GetAvailableRoomsAsync(int hotelId, int numberOfPeople, DateTime checkInDate, DateTime checkOutDate);



        //Reviews
        Task<ApiResponse<ReviewDTO>> AddHotelReviewAsync(CreateReviewDTO reviewDto);
        Task<ApiResponse<List<ReviewDTO>>> GetHotelReviewsAsync(int hotelId);
        Task<ApiResponse<ReviewDTO>> UpdateHotelReviewAsync(int reviewId, CreateReviewDTO reviewDto);
        Task<ApiResponse<bool>> RemoveHotelReviewAsync(int reviewId);

        Task<ApiResponse<ComplaintDTO>> AddHotelComplaintAsync(CreateComplaintDTO complaintDto);
        Task<ApiResponse<List<ComplaintDTO>>> GetHotelComplaintsAsync(int hotelId);


    }
}