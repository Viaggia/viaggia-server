namespace viaggia_server.Services.ImageService
{
    public interface IImageService
    {
        Task<string?> UploadImageAsync(IFormFile? file, string userId);
    }
}