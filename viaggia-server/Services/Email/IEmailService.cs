using viaggia_server.DTOs.Reserves;
using viaggia_server.Models.Reserves;

namespace viaggia_server.Services.Email
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string userName, string token);
        Task SendWelcomeEmailAsync(string email, string userName);
        Task<string> GetPasswordResetEmailTemplateAsync(string userName, string token, string validateTokenLink);
        Task SendApprovedReserve(Reserve reserve);
    }
}
