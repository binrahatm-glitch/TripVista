using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TripVista.Services.Interfaces;

namespace TripVista.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.Username))
            {
                // No SMTP credentials configured (local dev) - log instead of sending.
                _logger.LogInformation("EMAIL (not sent, no SMTP configured) To: {To} Subject: {Subject} Body: {Body}", toEmail, subject, body);
                return;
            }

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password)
            };

            using var message = new MailMessage(_settings.FromAddress, toEmail)
            {
                Subject = subject,
                Body = body
            };

            await client.SendMailAsync(message);
        }
    }
}
