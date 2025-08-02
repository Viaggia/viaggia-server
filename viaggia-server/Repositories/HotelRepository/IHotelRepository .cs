using viaggia_server.DTOs.HotelFilterDTO;
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
        Task<bool> NameExistsAsync(string name);
        Task<bool> CnpjExistsAsync(string? cnpj);
        Task<HotelRoomType> AddRoomTypeAsync(HotelRoomType roomType);
        Task<Media> AddMediaAsync(Media media);
        Task<Hotel?> GetHotelByNameAsync(string name);
        Task<IEnumerable<HotelRoomType>> GetHotelRoomTypesAsync(int hotelId);
        Task<IEnumerable<Media>> GetMediasByHotelIdAsync(int hotelId);
        Task<IEnumerable<Review>> GetReviewsByHotelIdAsync(int hotelId);
        Task<IEnumerable<Package>> GetPackagesByHotelIdAsync(int hotelId);
        Task<IEnumerable<Commoditie>> GetCommoditiesByHotelIdAsync(int hotelId);
        Task<IEnumerable<CommoditieServices>> GetCommoditieServicesByHotelIdAsync(int hotelId);
        Task<IEnumerable<Hotel>> GetHotelsWithRelatedDataAsync();

        //Task<CommoditieServices?> GetCommoditieServiceByIdAsync(int commoditieServiceId);
        //Task<Commoditie?> GetCommodityByIdAsync(int commoditieId);
        //Task<CommoditieServices> AddCommoditieServiceAsync(CommoditieServices commoditieService);
        //Task<Package?> GetPackageByIdAsync(int packageId);
        //Task<Commoditie> AddCommodityAsync(Commoditie commoditie);
        //Task<Review?> GetReviewByIdAsync(int reviewId);
        //Task<Package> AddPackageAsync(Package package);
        //Task<HotelRoomType?> GetRoomTypeByIdAsync(int roomTypeId);
        //Task<bool> UpdateRoomAvailabilityAsync(int roomTypeId, int roomsToReserve);
        //Task<Media?> GetMediaByIdAsync(int mediaId);
        //Task<bool> SoftDeleteMediaAsync(int mediaId);
        //Task<Review> AddReviewAsync(Review review);
    }
}