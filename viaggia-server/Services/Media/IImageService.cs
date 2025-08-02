namespace viaggia_server.Services.Media
{
    public interface IImageService
    {
        Task<string?> UploadImageAsync(IFormFile? image, string userId);
    }
}
