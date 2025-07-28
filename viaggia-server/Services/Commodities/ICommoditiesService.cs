using viaggia_server.DTOs.Commodities;

namespace viaggia_server.Services.Commodities
{
    public interface ICommoditiesService
    {
        Task<CommoditieDTO?> GetByHotelIdAsync(int hotelId);
        Task<List<CommoditieDTO>> GetAllAsync(); // Obtém todas as comodidades
        Task<CommoditieDTO> GetByIdAsync(int id); // Obtém uma comodidade específica pelo ID
        Task<CommoditieDTO> CreateAsync(CreateCommoditieDTO createCommoditieDTO);
        Task<CommoditieDTO> UpdateAsync(int id, CreateCommoditieDTO updateCommoditieDTO);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleActiveStatusAsync(int id); // Alterna o status ativo/inativo de uma comodidade
    }
}
