
using viaggia_server.Models.Commodities;
using viaggia_server.Models.CustomCommodities;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Reviews;


namespace viaggia_server.Repositories.HotelRepository
{
    public interface IHotelRepository
    {
        Task<bool> NameExistsAsync(string name);
        Task<bool> CnpjExistsAsync(string? cnpj);
        Task<HotelRoomType> AddRoomTypeAsync(HotelRoomType roomType);
        Task<Media> AddMediaAsync(Media media);
        Task<Hotel?> GetHotelByNameAsync(string name);
        Task<IEnumerable<HotelRoomType>> GetHotelRoomTypesAsync(int hotelId);
        Task<IEnumerable<Media>> GetMediasByHotelIdAsync(int hotelId);
        Task<IEnumerable<Review>> GetReviewsByHotelIdAsync(int hotelId);
        Task<IEnumerable<Package>> GetPackagesByHotelIdAsync(int hotelId);
        Task<IEnumerable<Commodity>> GetCommodityByHotelIdAsync(int hotelId);
        Task<IEnumerable<CustomCommodity>> GetCustomCommodityByHotelIdAsync(int hotelId);
        Task<IEnumerable<Hotel>> GetHotelsWithRelatedDataAsync();
        Task<IEnumerable<HotelRoomType>> GetAvailableRoomTypesAsync(int hotelId, int numberOfPeople, DateTime checkInDate, DateTime checkOutDate);
  
    }
}