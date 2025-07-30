using viaggia_server.DTOs.Commoditie;
using viaggia_server.DTOs.Commodity;


namespace viaggia_server.Services.Commodities
{
    public interface ICommoditieService
    {
        Task<CommoditieDTO?> GetByHotelIdAsync(int hotelId);
        Task<bool> ToggleActiveStatusAsync(int id); // Alterna o status ativo/inativo de uma comodidade
    }
}
