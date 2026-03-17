using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace TechPro.API.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailOptions _opt;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<EmailOptions> opt, ILogger<SmtpEmailSender> logger)
        {
            _opt = opt.Value;
            _logger = logger;
        }

        public async Task<bool> SendAsync(string toEmail, string subject, string body, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(_opt.Host) || string.IsNullOrWhiteSpace(_opt.FromEmail))
            {
                _logger.LogInformation("[Email disabled] To={To} Subject={Subject}", toEmail, subject);
                return false;
            }

            try
            {
                using var msg = new MailMessage
                {
                    From = new MailAddress(_opt.FromEmail, _opt.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                msg.To.Add(toEmail);

                using var smtp = new SmtpClient(_opt.Host, _opt.Port)
                {
                    EnableSsl = _opt.EnableSsl
                };

                if (!string.IsNullOrWhiteSpace(_opt.Username))
                {
                    smtp.Credentials = new NetworkCredential(_opt.Username, _opt.Password);
                }
                else
                {
                    smtp.UseDefaultCredentials = true;
                }

                // SmtpClient không hỗ trợ CancellationToken; chạy async và tôn trọng ct trước khi gửi.
                ct.ThrowIfCancellationRequested();
                await smtp.SendMailAsync(msg);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Email send failed");
                return false;
            }
        }
    }
}

