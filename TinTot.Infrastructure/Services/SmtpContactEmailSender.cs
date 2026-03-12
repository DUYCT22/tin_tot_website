using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using TinTot.Application.Interfaces.Contact;
using TinTot.Domain.Entities;

namespace TinTot.Infrastructure.Services
{
    public class SmtpContactEmailSender : IContactEmailSender
    {
        private readonly SmtpOptions _smtpOptions;

        public SmtpContactEmailSender(IConfiguration configuration)
        {
            _smtpOptions = configuration.GetSection("Smtp").Get<SmtpOptions>() ?? new SmtpOptions();
        }

        public async Task SendAsync(ContactMessage message, string destinationEmail)
        {
            if (string.IsNullOrWhiteSpace(_smtpOptions.Host) || string.IsNullOrWhiteSpace(_smtpOptions.FromEmail))
            {
                throw new InvalidOperationException("Thiếu cấu hình SMTP. Vui lòng cấu hình Smtp trong appsettings.");
            }

            using var smtpClient = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port)
            {
                EnableSsl = _smtpOptions.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(_smtpOptions.Username))
            {
                smtpClient.Credentials = new NetworkCredential(_smtpOptions.Username, _smtpOptions.Password);
            }

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpOptions.FromEmail, _smtpOptions.FromName),
                Subject = $"[Tin Tốt - Liên hệ] {message.FullName}",
                Body = $"""
Họ và tên: {message.FullName}
Email người gửi: {message.SenderEmail}
Thời gian (UTC): {message.CreatedAtUtc:yyyy-MM-dd HH:mm:ss}

Vấn đề cần liên hệ:
{message.Issue}
""",
                IsBodyHtml = false
            };

            mailMessage.To.Add(destinationEmail);
            mailMessage.ReplyToList.Add(new MailAddress(message.SenderEmail));

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
