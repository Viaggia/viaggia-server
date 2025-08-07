using viaggia_server.DTOs.Hotel;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.CustomCommodities;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Reserves;
using viaggia_server.Models.Reviews;


namespace viaggia_server.Repositories.HotelRepository
{
    public interface IHotelRepository
    {
        Task UpdateAsync(Hotel hotel);
        Task<Hotel?> GetByIdAsync(int id); // Add this method
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

        //Task<CommoditieServices?> GetCommoditieServiceByIdAsync(int commoditieServiceId);
        //Task<Commoditie?> GetCommodityByIdAsync(int commoditieId);
        //Task<CommoditieServices> AddCommoditieServiceAsync(CommoditieServices commoditieService);
        Task<Package?> GetPackageByIdAsync(int packageId);
        //Task<Commoditie> AddCommodityAsync(Commoditie commoditie);
        //Task<Review?> GetReviewByIdAsync(int reviewId);
        //Task<Package> AddPackageAsync(Package package);
        //Task<HotelRoomType?> GetRoomTypeByIdAsync(int roomTypeId);
        Task<bool> UpdateRoomAvailabilityAsync(int roomTypeId, int roomsToReserve);
        //Task<Media?> GetMediaByIdAsync(int mediaId);
        //Task<bool> SoftDeleteMediaAsync(int mediaId);
        Task<Review> AddReviewAsync(Review review);
        Task<List<HotelBalanceDTO>> GetBalancesHotelsAsync();

        Task UpdateRoomTypeAsync(HotelRoomType roomType);
        Task<HotelRoomType?> GetRoomTypeByIdAsync(int roomTypeId);
        Task<Hotel?> GetHotelByIdWithDetailsAsync(int hotelId);
        Task<IEnumerable<HotelRoomType>> GetAvailableRoomTypesAsync(int hotelId, int numberOfPeople, DateTime checkInDate, DateTime checkOutDate);
        Task<IEnumerable<Hotel>> GetHotelsByUserIdAsync(int userId);
        Task<IEnumerable<Reserve>> GetReservationsByHotelIdAsync(int hotelId);
        Task<IEnumerable<Hotel>> GetAvailableHotelsByDestinationAsync(
              string city,
              int numberOfPeople,
              int numberOfRooms,
              DateTime checkInDate,
              DateTime checkOutDate);

        Task<Hotel> GetByIdHotel(int  hotelId);
    }
}