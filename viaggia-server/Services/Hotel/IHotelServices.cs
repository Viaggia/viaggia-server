using viaggia_server.DTOs;
using viaggia_server.DTOs.Hotel;
using viaggia_server.DTOs.Hotels;
using viaggia_server.DTOs.Packages;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;

namespace viaggia_server.Services.HotelServices
{
    public interface IHotelServices
    {
        Task<ApiResponse<List<HotelDTO>>> GetAllHotelAsync();
        Task<Hotel?> GetHotelByIdAsync(int id);
        Task<ApiResponse<Hotel>> CreateHotelAsync(CreateHotelDTO createHotelDto, List<CreateHotelRoomTypeDTO> roomTypes);
        Task<Hotel> UpdateHotelAsync(UpdateHotelDto updateHotelDto);
        Task<bool> SoftDeleteHotelAsync(int id);

        Task<ApiResponse<double>> GetHotelAverageRatingAsync(int hotelId);
        Task<ApiResponse<IEnumerable<PackageDTO>>> GetPackagesByHotelIdAsync(int hotelId);
    }
}