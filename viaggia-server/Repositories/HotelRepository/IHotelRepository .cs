using viaggia_server.Models.Commodities;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Reviews;

namespace viaggia_server.Repositories.HotelRepository
{
    public interface IHotelRepository
    {
        // Existing methods
        Task<bool> NameExistsAsync(string name);
        Task<bool> CnpjExistsAsync(string? cnpj);


        // 
        Task<HotelRoomType> AddRoomTypeAsync(HotelRoomType roomType);
        Task<Media> AddMediaAsync(Media media);

        Task<Hotel?> GetHotelByNameAsync(string name);
        Task<IEnumerable<HotelRoomType>> GetHotelRoomTypesAsync(int hotelId);
        Task<HotelRoomType?> GetRoomTypeByIdAsync(int roomTypeId);
        Task<IEnumerable<Media>> GetMediasByHotelIdAsync(int hotelId);


        Task<bool> UpdateRoomAvailabilityAsync(int roomTypeId, int roomsToReserve);
        Task<Media?> GetMediaByIdAsync(int mediaId);
        Task<bool> SoftDeleteMediaAsync(int mediaId);
        Task<Review> AddReviewAsync(Review review);
        Task<IEnumerable<Review>> GetReviewsByHotelIdAsync(int hotelId);
        Task<Review?> GetReviewByIdAsync(int reviewId);
        Task<Package> AddPackageAsync(Package package);
        Task<IEnumerable<Package>> GetPackagesByHotelIdAsync(int hotelId);
        Task<Package?> GetPackageByIdAsync(int packageId);
        Task<Commoditie> AddCommodityAsync(Commoditie commoditie);
        Task<IEnumerable<Commoditie>> GetCommoditiesByHotelIdAsync(int hotelId);
        Task<Commoditie?> GetCommodityByIdAsync(int commoditieId);
        Task<CommoditieServices> AddCommoditieServiceAsync(CommoditieServices commoditieService);
        Task<IEnumerable<CommoditieServices>> GetCommoditieServicesByHotelIdAsync(int hotelId);
        Task<CommoditieServices?> GetCommoditieServiceByIdAsync(int commoditieServiceId);
    }
}