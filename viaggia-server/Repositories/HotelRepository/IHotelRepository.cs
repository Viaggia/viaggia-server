using viaggia_server.Models.Hotels;
using viaggia_server.Models.Addresses;
using viaggia_server.Models.HotelRoomTypes;

namespace viaggia_server.Repositories.HotelRepository
{
    public interface IHotelRepository
    {
        // Reativar um hotel
        Task<bool> ReactivateAsync(int id);
        // Verifica se um hotel já existe pelo nome
        Task<bool> NameExistsAsync(string nome);
        Task<bool> CnpjExistsAsync(string? cnpj);

        // Endereços
        Task<Address> AddAddressAsync(Address address);
        Task<Address?> GetAddressByIdAsync(int addressId);
        Task<Address?> GetAddressByHotelIdAsync(int hotelId);

        // Tipos de Quarto
        Task<HotelRoomType> AddRoomTypeAsync(HotelRoomType roomType);
        Task<HotelRoomType?> GetRoomTypeByIdAsync(int roomTypeId);
        Task<IEnumerable<HotelRoomType>> GetHotelRoomTypesAsync(int hotelId);
    }
}









