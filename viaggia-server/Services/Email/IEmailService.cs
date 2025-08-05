namespace viaggia_server.Services.Email
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string userName, string token);
        Task SendWelcomeEmailAsync(string email, string userName);
        Task<string> GetPasswordResetEmailTemplateAsync(string userName, string token, string validateTokenLink);
        Task SendApprovedReserve(string email, string userName, int ReservationId, string Hotel, DateTime checkIn, DateTime checkOut, string hotelEmail, string hotelPhone);
    }
}
