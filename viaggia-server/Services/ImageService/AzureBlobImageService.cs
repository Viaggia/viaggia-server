using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace viaggia_server.Services.ImageService
{
    public class AzureBlobImageService : IImageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<AzureBlobImageService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _containerName;

        public AzureBlobImageService(
            BlobServiceClient blobServiceClient, 
            ILogger<AzureBlobImageService> logger,
            IConfiguration configuration)
        {
            _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _containerName = _configuration["AzureBlob:ContainerName"] ?? "viaggia-media";
        }

        public async Task<string?> UploadImageAsync(IFormFile file, string userId)
        {
            if (file == null || file.Length == 0)
                return null;

            try
            {
                // Validate file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                    throw new ArgumentException("Invalid image format. Only jpg, jpeg, png, and gif are allowed.");

                if (file.Length > 10 * 1024 * 1024) // 10MB for Azure
                    throw new ArgumentException("Image size exceeds 10MB.");

                // Generate unique blob name
                var blobName = $"avatars/{userId}_{Guid.NewGuid()}{extension}";

                // Get container client
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                
                // Ensure container exists
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                // Get blob client
                var blobClient = containerClient.GetBlobClient(blobName);

                // Set content type based on file extension
                var contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    _ => "application/octet-stream"
                };

                // Upload file
                using var stream = file.OpenReadStream();
                var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };
                await blobClient.UploadAsync(stream, blobHttpHeaders, overwrite: true);

                _logger.LogInformation("Successfully uploaded image to blob: {BlobName}", blobName);

                // Return the blob URL
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Azure Blob Storage for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return false;

                // Extract blob name from URL
                var uri = new Uri(imageUrl);
                var blobName = uri.Segments.Skip(2).Aggregate((a, b) => a + b).TrimStart('/');

                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                var response = await blobClient.DeleteIfExistsAsync();
                
                _logger.LogInformation("Deleted blob: {BlobName}, Success: {Success}", blobName, response.Value);
                
                return response.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image from Azure Blob Storage: {ImageUrl}", imageUrl);
                return false;
            }
        }
    }
}
