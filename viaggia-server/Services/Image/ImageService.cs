namespace viaggia_server.Services
{
    public interface IImageService
    {
        Task<string?> UploadImageAsync(IFormFile? image, string userId);
    }
}
