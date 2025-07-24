namespace viaggia_server.DTOs.Packages
{
    public class PackageUpdateDTO
    {
        public int PackageId { get; set; }
        public string? Name { get; set; }
        public string? Destination { get; set; }
        public string? Description { get; set; }
        public decimal? BasePrice { get; set; }
        public bool IsActive { get; set; }
        public List<IFormFile> NewMediaFiles { get; set; } = new List<IFormFile>(); // Novos arquivos
        public List<int> MediaIdsToDelete { get; set; } = new List<int>(); // IDs de mídias a remover
        public List<PackageDateDTO> PackageDates { get; set; } = new List<PackageDateDTO>();
    }
}
