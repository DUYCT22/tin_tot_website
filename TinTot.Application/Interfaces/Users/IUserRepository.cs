using TinTot.Domain.Entities;
namespace TinTot.Application.Interfaces.Users
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByLoginNameAsync(string loginName);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneAsync(string phone);
        Task<List<string?>> GetAllAvatarUrlsAsync();

        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);

        Task SaveChangesAsync();
    }
}
