namespace viaggia_server.Services.Media
{
    public class ImageService : IImageService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IConfiguration configuration, ILogger<ImageService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string?> UploadImageAsync(IFormFile? image, string userId)
        {
            if (image == null)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                // Return URL relative to the application
                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
                return $"{baseUrl}/uploads/avatars/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image for user {UserId}", userId);
                throw new Exception("Failed to upload image.");
            }
        }
    }
}