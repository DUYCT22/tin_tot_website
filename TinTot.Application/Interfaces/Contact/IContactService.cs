using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Application.DTOs.Contact;

namespace TinTot.Application.Interfaces.Contact
{
    public interface IContactService
    {
        Task SendContactAsync(ContactRequestDto request);
    }
}
