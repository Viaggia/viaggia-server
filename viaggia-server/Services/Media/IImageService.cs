namespace viaggia_server.Services.Medias
{
    public interface IImageService
    {
        Task<string?> UploadImageAsync(IFormFile? image, string userId);
    }
}