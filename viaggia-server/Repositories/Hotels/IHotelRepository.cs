using viaggia_server.Models.Hotels;

namespace viaggia_server.Repositories.Hotels
{
    public interface IHotelRepository
    {
        // Criar um hotel
        Task<Hotel> CreateAsync(Hotel hotel);

        // Aguarda a aprovação do hotel
        Task<Hotel?> StatusHotel(int id);

        // Reativar um hotel
        Task<bool> ReactivateAsync(int id);

        // Verifica se um hotel já existe pelo nome
        Task<bool> NomeExistsAsync(string nome);


    }
}
