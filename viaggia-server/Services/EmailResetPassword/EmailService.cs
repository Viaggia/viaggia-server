using System.Net;
using System.Net.Mail;
using viaggia_server.Models.Reserves;
using viaggia_server.Repositories;

namespace viaggia_server.Services.EmailResetPassword
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly IWebHostEnvironment _environment;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger, IWebHostEnvironment environment)
        {
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> getBeWelcomeViaggia(string userName)
        {
            // Caminho para o template
            var templatePath = Path.Combine(_environment.ContentRootPath, "templates", "BeWelcomeTamplete.html");

            // Verificar se o arquivo existe
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template de email não encontrado: {templatePath}");
            }

            // Ler o conteúdo do template
            var templateContent = await File.ReadAllTextAsync(templatePath);

            // Substituir os placeholders pelos valores reais
            var htmlContent = templateContent.Replace("{{UserName}}", userName);

            return htmlContent;
        }

        public async Task SendWelcomeEmailAsync(string email, string userName)
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
            var subject = "🎉 Bem-vindo ao Viaggia!";

            // Obter o conteúdo HTML do template
            var htmlContent = await getBeWelcomeViaggia(userName);

            var mailMessage = new MailMessage(from, to)
            {
                Subject = subject,
                Body = htmlContent,
                IsBodyHtml = true
            };

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email de boas-vindas enviado para {Email} (usuário: {UserName})", email, userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar email de boas-vindas para {Email}", email);
                throw new Exception("Falha ao enviar o e-mail de boas-vindas.");
            }
        }

        public async Task SendPasswordResetEmailAsync(string email, string userName, string token)
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
            var subject = "🔑 Redefinição de Senha - Viaggia";

            // Link para validar token
            var validateTokenLink = $"http://localhost:5173/validate-token?token={token}";

            // Obter o conteúdo HTML do template
            var htmlContent = await GetPasswordResetEmailTemplateAsync(userName, token, validateTokenLink);

            var mailMessage = new MailMessage(from, to)
            {
                Subject = subject,
                Body = htmlContent,
                IsBodyHtml = true
            };

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email de reset de senha enviado para {Email} (usuário: {UserName})", email, userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar email de reset para {Email}", email);
                throw new Exception("Falha ao enviar o e-mail de redefinição de senha.");
            }
        }

        // método para construção do envio para recuperar senha
        public async Task<string> GetPasswordResetEmailTemplateAsync(string userName, string token, string validateTokenLink)
        {
            // Caminho para o template
            var templatePath = Path.Combine(_environment.ContentRootPath, "templates", "PasswordResetEmailTemplate.html");

            // Verificar se o arquivo existe
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template de email não encontrado: {templatePath}");
            }

            // Ler o conteúdo do template
            var templateContent = await File.ReadAllTextAsync(templatePath);

            // Substituir os placeholders pelos valores reais
            var htmlContent = templateContent
                .Replace("{{UserName}}", userName)
                .Replace("{{Token}}", token)
                .Replace("{{ValidateTokenLink}}", validateTokenLink);

            return htmlContent;
        }

        //public async Task SendApprovedReserve(Reserve reserve)
        //{
        //    var email = reserve.User.Email;
        //    _logger.LogInformation("{email}", email);
        //    // Verificação de integridade do objeto antes de enviar o e-mail
        //    if (reserve.User == null)
        //    {
        //        _logger.LogError("Tentativa de envio de e-mail de aprovação falhou: 'User' da reserva está nulo.");
        //        throw new ArgumentNullException(nameof(reserve.User), "Usuário da reserva não pode ser nulo.");
        //    }

        //    if (string.IsNullOrWhiteSpace(reserve.User.Email))
        //    {
        //        _logger.LogError("Tentativa de envio de e-mail de aprovação falhou: 'Email' do usuário está vazio ou nulo.");
        //        throw new ArgumentException("Email do usuário da reserva está vazio ou nulo.");
        //    }

        //    var smtpClient = new SmtpClient(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"]))
        //    {
        //        Credentials = new NetworkCredential(
        //            _configuration["Smtp:Username"],
        //            _configuration["Smtp:Password"]),
        //        EnableSsl = true
        //    };

        //    var from = new MailAddress(
        //        _configuration["Smtp:FromEmail"],
        //        _configuration["Smtp:FromName"]);
        //    var to = new MailAddress(reserve.User.Email);
        //    _logger.LogInformation("{to}", to);
        //    var subject = "Reserva Aprovada - Viaggia";

        //    // Obter o conteúdo HTML do template
        //    var htmlContent = await GetApprovedReserve(reserve);

        //    var mailMessage = new MailMessage(from, to)
        //    {
        //        Subject = subject,
        //        Body = htmlContent,
        //        IsBodyHtml = true
        //    };

        //    try
        //    {
        //        await smtpClient.SendMailAsync(mailMessage);
        //        _logger.LogInformation("Email de aprovação de reserva enviado para {Email} (usuário: {UserName})", reserve.User.Email, reserve.User.Name);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Falha ao enviar e-mail de aprovação para {Email}", reserve.User.Email);
        //        throw new Exception($"Falha ao enviar o e-mail de aprovação para {reserve.User.Email}: {ex.Message}", ex);
        //    }
        //}


    }


}