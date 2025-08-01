using viaggia_server.DTOs.Commoditie;

namespace viaggia_server.Services.Commodities
{
    public interface ICommoditieService
    {
        Task<List<CommoditieDTO>> GetAllAsync();
        Task<CommoditieDTO?> GetByIdAsync(int id);
        Task<CommoditieDTO?> GetByHotelIdAsync(int hotelId);
        Task<IEnumerable<CommoditieDTO>> GetByHotelIdListAsync(int hotelId);
        Task<CommoditieDTO> CreateAsync(CreateCommoditieDTO createDto);
        Task<CommoditieDTO> UpdateAsync(int id, CreateCommoditieDTO updateDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleActiveStatusAsync(int id);
    }
}