using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Domain.Entities;
namespace TinTot.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByLoginNameAsync(string loginName);
        Task<User?> GetByEmailAsync(string email);

        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);

        Task SaveChangesAsync();
    }
}
