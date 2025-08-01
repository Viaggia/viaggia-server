using viaggia_server.DTOs.Hotel;
using viaggia_server.DTOs.Hotels;
using viaggia_server.DTOs.User;
using viaggia_server.DTOs.Users;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.HotelDates;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Reviews;

namespace viaggia_server.Repositories.HotelRepository
{
    public interface IHotelRepository 
    {
        // Criar um hotel
        Task<Hotel?> CreateAsync(Hotel hotel);
        // Aguarda a aprovação do hotel
        Task<Hotel> StatusHotel(int id);
        // Reativar um hotel pelo ID
        Task<bool> ReactivateAsync(int id);
        // Verifica se um hotel já existe pelo nome
        Task<bool> NameExistsAsync(string name);
        Task<bool> CnpjExistsAsync(string? cnpj);
        // Tipos de Quarto
        Task<HotelRoomType> AddRoomTypeAsync(HotelRoomType roomType);
        Task<HotelRoomType?> GetRoomTypeByIdAsync(int roomTypeId);
        Task<IEnumerable<HotelRoomType>> GetHotelRoomTypesAsync(int hotelId);
        // Hotel Dates
        Task<IEnumerable<HotelDate>> GetHotelDatesAsync(int hotelId);
        Task<HotelDate?> GetHotelDateByIdAsync(int hotelDateId);
        Task<HotelDate> AddHotelDateAsync(HotelDate hotelDate);
        // Medias
        Task<IEnumerable<Media>> GetMediasByHotelIdAsync(int hotelId);
        Task<Media?> GetMediaByIdAsync(int mediaId);
        Task<Media> AddMediaAsync(Media media);
        Task<bool> SoftDeleteMediaAsync(int mediaId);

        // Reviews
        Task<IEnumerable<Review>> GetReviewsByHotelIdAsync(int hotelId);
        Task<Review?> GetReviewByIdAsync(int reviewId);
        Task<Review> AddReviewAsync(Review review);
        // Packages
        Task<IEnumerable<Package>> GetPackagesByHotelIdAsync(int hotelId);
        Task<Package?> GetPackageByIdAsync(int packageId);
        Task<Package> AddPackageAsync(Package package);

        // Commodities
        Task<IEnumerable<Commoditie>> GetCommoditiesByHotelIdAsync(int hotelId);
        Task<Commoditie?> GetCommoditieByIdAsync(int commoditieId);
        Task<Commoditie> AddCommoditieAsync(Commoditie commoditie);


        // CommoditieServices
        Task<IEnumerable<CommoditieServices>> GetCommoditieServicesByHotelIdAsync(int hotelId);
        Task<CommoditieServices?> GetCommoditieServiceByIdAsync(int commoditieServiceId);
        Task<CommoditieServices> AddCommoditieServiceAsync(CommoditieServices commoditieService);

        //Hotel
        Task<Hotel?> GetHotelByNameAsync(string name);

       






    }






}









