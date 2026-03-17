using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinTot.Application.Interfaces.Users
{
    public interface IPasswordResetEmailSender
    {
        Task SendResetCodeAsync(string email, string code);
    }
}
