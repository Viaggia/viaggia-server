namespace viaggia_server.Services.Auth
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string email, string senha);
    }

}
