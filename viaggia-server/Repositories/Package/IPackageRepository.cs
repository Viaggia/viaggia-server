using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;

namespace viaggia_server.Repositories
{
    public interface IPackageRepository
    {

        Task<IEnumerable<PackageDate>> GetPackageDatesAsync(int packageId);
        Task<PackageDate?> GetPackageDateByIdAsync(int packageDateId);
        Task<IEnumerable<Media>> GetPackageMediasAsync(int packageId);
        Task<Media> AddMediaAsync(Media media);
        Task<bool> DeleteMediaAsync(int mediaId);
        Task<IEnumerable<Package>> SearchPackagesByDestinationAndDateAsync(string destination, DateTime startDate, DateTime endDate);

        Task<PackageDate> AddPackageDateAsync(PackageDate packageDate);
    

    }
}