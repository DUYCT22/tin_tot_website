using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using TinTot.Application.Interfaces.Users;

namespace TinTot.Infrastructure.Services
{
    public class SmtpPasswordResetEmailSender : IPasswordResetEmailSender
    {
        private readonly SmtpOptions _smtpOptions;

        public SmtpPasswordResetEmailSender(IConfiguration configuration)
        {
            _smtpOptions = configuration.GetSection("Smtp").Get<SmtpOptions>() ?? new SmtpOptions();
        }

        public async Task SendResetCodeAsync(string email, string code)
        {
            if (string.IsNullOrWhiteSpace(_smtpOptions.Host) || string.IsNullOrWhiteSpace(_smtpOptions.FromEmail))
            {
                throw new InvalidOperationException("SMTP configuration is missing.");
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
                Subject = "Mã xác nhận đặt lại mật khẩu",
                Body = $"Mã xác thực của bạn là: <b>{WebUtility.HtmlEncode(code)}</b>. Mã có hiệu lực trong 5 phút.",
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
