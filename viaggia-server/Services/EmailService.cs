using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace viaggia_server.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string email, string token)
        {
            var smtpClient = new SmtpClient(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"]))
            {
                Credentials = new NetworkCredential(
                    _configuration["Smtp:Username"],
                    _configuration["Smtp:Password"]),
                EnableSsl = true
            };

            var from = new MailAddress(
                _configuration["Smtp:FromEmail"],
                _configuration["Smtp:FromName"]);
            var to = new MailAddress(email);
            var subject = "Redefinição de Senha - Viaggia";
            var resetLink = $"http://localhost:5173/reset-password?token={token}"; // Adjust for your frontend
            var htmlContent = $"<p>Olá,</p><p>Você solicitou a redefinição de sua senha. Clique no link abaixo para redefinir sua senha:</p><p><a href='{resetLink}'>Redefinir Senha</a></p><p>Este link expira em 1 hora. Se você não solicitou isso, ignore este email.</p>";

            var mailMessage = new MailMessage(from, to)
            {
                Subject = subject,
                Body = htmlContent,
                IsBodyHtml = true
            };

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent to {Email} via Gmail SMTP", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
                throw new Exception("Falha ao enviar o e-mail de redefinição de senha.");
            }
        }
    }
}