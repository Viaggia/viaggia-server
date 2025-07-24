using System.Text.Json;
using System.Text.Json.Serialization;

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
                                                                                 // Alterado para string que será desserializada
        public string PackageDatesJson { get; set; } = "[]";

        // Propriedade auxiliar para acessar as datas desserializadas
        [JsonIgnore]
        public List<PackageDateDTO> PackageDates =>
            JsonSerializer.Deserialize<List<PackageDateDTO>>(PackageDatesJson) ?? new List<PackageDateDTO>();
    }
}

