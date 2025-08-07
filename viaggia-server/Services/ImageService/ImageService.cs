namespace viaggia_server.Services.ImageService
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IWebHostEnvironment environment, ILogger<ImageService> logger)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string?> UploadImageAsync(IFormFile file, string userId)
        {
            if (file == null || file.Length == 0)
                return null;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid image format. Only jpg, jpeg, and png are allowed.");

            if (file.Length > 5 * 1024 * 1024) // 5MB
                throw new ArgumentException("Image size exceeds 5MB.");

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "avatars");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Retorna a URL relativa para a imagem
            return $"/avatars/{fileName}";
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return false;

                // Extract filename from URL
                var fileName = Path.GetFileName(imageUrl);
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "avatars");
                var filePath = Path.Combine(uploadsFolder, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted local file: {FilePath}", filePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting local image file: {ImageUrl}", imageUrl);
                return false;
            }
        }
    }
}