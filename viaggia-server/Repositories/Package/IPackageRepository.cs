
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;

namespace viaggia_server.Repositories
{
    public interface IPackageRepository
    {
        Task<IEnumerable<PackageDate>> GetPackageDatesAsync(int packageId); // Obter datas de um pacote
        Task<PackageDate?> GetPackageDateByIdAsync(int packageDateId); // Obter data específica
        Task<IEnumerable<Media>> GetPackageMediasAsync(int packageId); // Obter mídias de um pacote
        Task<Media> AddMediaAsync(Media media); // Adicionar mídia
        Task<bool> DeleteMediaAsync(int mediaId); // Remover mídia
    }
}