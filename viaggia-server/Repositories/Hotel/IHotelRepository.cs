using viaggia_server.Models.Addresses;
using viaggia_server.Models.HotelDates;
using viaggia_server.Models.HotelRoomTypes;

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
       

    }
}









