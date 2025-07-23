namespace viaggia_server.DTOs.Packages
{
    public class PackageCreateDTO
    {
        public string? Name { get; set; }
        public string? Destination { get; set; }
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsActive { get; set; }
        public List<IFormFile> MediaFiles { get; set; } = new List<IFormFile>(); // Para upload de arquivos
        public List<PackageDateDTO> PackageDates { get; set; } = new List<PackageDateDTO>();
    }
}
