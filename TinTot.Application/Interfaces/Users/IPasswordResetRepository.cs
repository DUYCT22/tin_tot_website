using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Domain.Entities;

namespace TinTot.Application.Interfaces.Users
{
    public interface IPasswordResetRepository
    {
        Task<PasswordResetCode?> GetByEmailAsync(string email);
        Task UpsertAsync(PasswordResetCode passwordResetCode);
        Task RemoveAsync(string email);
    }
}
