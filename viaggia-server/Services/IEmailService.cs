namespace viaggia_server.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string userName, string token);
        Task SendWelcomeEmailAsync(string email, string userName);
        Task<string> GetPasswordResetEmailTemplateAsync(string userName, string token, string validateTokenLink);
    }
}
