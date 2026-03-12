using Microsoft.Extensions.Configuration;
using TinTot.Application.DTOs.Contact;
using TinTot.Application.Interfaces.Contact;
using TinTot.Domain.Entities;

namespace TinTot.Application.Services.Contact
{
    public class ContactService : IContactService
    {
        private readonly IContactEmailSender _contactEmailSender;
        private readonly string _destinationEmail;

        public ContactService(IContactEmailSender contactEmailSender, IConfiguration configuration)
        {
            _contactEmailSender = contactEmailSender;
            _destinationEmail = configuration["Contact:DestinationEmail"] ?? "nguyennhutduy.cv@gmail.com";
        }

        public async Task SendContactAsync(ContactRequestDto request)
        {
            var message = new ContactMessage
            {
                SenderEmail = request.SenderEmail.Trim(),
                FullName = request.FullName.Trim(),
                Issue = request.Issue.Trim(),
                CreatedAtUtc = DateTime.UtcNow
            };

            await _contactEmailSender.SendAsync(message, _destinationEmail);
        }
    }
}
