using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Domain.Entities;

namespace TinTot.Application.Interfaces.Contact
{
    public interface IContactEmailSender
    {
        Task SendAsync(ContactMessage message, string destinationEmail);
    }
}
