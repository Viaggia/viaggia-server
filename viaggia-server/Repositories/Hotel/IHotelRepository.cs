using viaggia_server.Models.Addresses;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.HotelDates;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Reviews;

namespace viaggia_server.Repositories.Hotel
{
    public interface IHotelRepository
    {
        // Endereços
        Task<Address> AddAddressAsync(Address address);
        Task<Address?> GetAddressByIdAsync(int addressId);
        Task<Address?> GetAddressByHotelIdAsync(int hotelId);

        // Tipos de Quarto
        Task<HotelRoomType> AddRoomTypeAsync(HotelRoomType roomType);
        Task<HotelRoomType?> GetRoomTypeByIdAsync(int roomTypeId);
        Task<IEnumerable<HotelRoomType>> GetHotelRoomTypesAsync(int hotelId);

        // Hotel Dates
        Task<HotelDate> AddHotelDateAsync(HotelDate hotelDate);
        Task<HotelDate?> GetHotelDateByIdAsync(int hotelDateId);
        Task<IEnumerable<HotelDate>> GetHotelDatesAsync(int hotelId);
       
        //Medias
        Task<Media> AddMediaAsync(Media media);
        Task<bool> DeleteMediaAsync(int mediaId);
        Task<IEnumerable<Media>> GetHotelMediasAsync(int hotelId);

        //Reviews
        Task<Review> AddReviewAsync(Review review);
        Task<Review?> GetReviewByIdAsync(int reviewId);
        Task<IEnumerable<Review>> GetHotelReviewsAsync(int hotelId);

        //pacotes
        Task<Package> AddPackageAsync(Package package);
        Task<Package?> GetPackageByIdAsync(int packageId);
        Task<IEnumerable<Package>> GetHotelPackagesAsync(int hotelId);

        //commodities
        Task<Commoditie> AddCommoditieAsync(Commoditie commoditie); 
        Task<Commoditie?> GetCommoditieByIdAsync(int commoditieId); 
        Task<Commoditie?> GetCommoditieByHotelIdAsync(int hotelId);
        Task<IEnumerable<Commoditie>> GetAllCommoditiesAsync(); 
        Task<bool> UpdateCommoditieAsync(Commoditie commoditie);
        Task<bool> DeleteCommoditieAsync(int commoditieId);
        Task<IEnumerable<Commoditie>> GetHotelCommoditiesAsync(int hotelId); 

        

        // Pesquisa de Hotéis
        //Task<IEnumerable<Hotel>> SearchHotelsByDestinationAndDateAsync(string destination, DateTime startDate, DateTime endDate);

        // Soft Delete
        //Task<bool> SoftDeleteAddressAsync(int addressId);
        //Task<bool> SoftDeleteRoomTypeAsync(int roomTypeId);
        // Task<bool> SoftDeleteHotelDateAsync(int hotelDateId);

    }
}









