using viaggia_server.Models.Hotels;
using viaggia_server.Models.Addresses;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Models.HotelDates;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Reviews;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Commodities;

namespace viaggia_server.Repositories.HotelRepository
{
    public interface IHotelRepository 
    {
        // Reativar um hotel
        Task<bool> ReactivateAsync(int id);
        // Verifica se um hotel já existe pelo nome
        Task<bool> NameExistsAsync(string nome);
        Task<bool> CnpjExistsAsync(string? cnpj);

       
        // Tipos de Quarto
        Task<HotelRoomType> AddRoomTypeAsync(HotelRoomType roomType);
        Task<HotelRoomType?> GetRoomTypeByIdAsync(int roomTypeId);
        Task<IEnumerable<HotelRoomType>> GetHotelRoomTypesAsync(int hotelId);

        //Hotel Dates
        Task<IEnumerable<HotelDate>> GetHotelDatesAsync(int hotelId);
        Task<HotelDate?> GetHotelDateByIdAsync(int hotelDateId);
        Task<HotelDate> AddHotelDateAsync(HotelDate hotelDate);

        //Medias
        Task<IEnumerable<Media>> GetMediasByHotelIdAsync(int hotelId);
        Task<Media?> GetMediaByIdAsync(int mediaId);
        Task<Media> AddMediaAsync(Media media);

        //Reviews
        Task<IEnumerable<Review>> GetReviewsByHotelIdAsync(int hotelId);
        Task<Review?> GetReviewByIdAsync(int reviewId);
        Task<Review> AddReviewAsync(Review review);

        //Packages
        Task<IEnumerable<Package>> GetPackagesByHotelIdAsync(int hotelId);
        Task<Package?> GetPackageByIdAsync(int packageId);
        Task<Package> AddPackageAsync(Package package);

        






    }
}









