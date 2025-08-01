using viaggia_server.DTOs;
using viaggia_server.DTOs.Hotel;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;


namespace viaggia_server.Services.HotelServices
{
    public interface IHotelServices
    {
        Task<IEnumerable<Hotel>> GetAllHotelsAsync();
        Task<Hotel?> GetHotelByIdAsync(int id);
        Task<Hotel> AddHotelAsync(Hotel hotel);
        Task<Hotel> UpdateHotelAsync(UpdateHotelDto updateHotelDto);
        Task<bool> SoftDeleteHotelAsync(int id);
        Task<bool> SaveChangesAsync();

        // New methods for media management
        Task<IEnumerable<Media>> GetMediaByHotelIdAsync(int hotelId);
        Task<Media?> GetMediaByIdAsync(int mediaId);
        Task<Media> AddMediaToHotelAsync(Media mediaDto);
        Task<bool> SoftDeleteMediaAsync(int mediaId);


    }
}
