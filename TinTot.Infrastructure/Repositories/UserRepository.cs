
using Microsoft.EntityFrameworkCore;
using TinTot.Infrastructure.Data;
using TinTot.Domain.Entities;
using TinTot.Application.Interfaces.Users;
namespace TinTot.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetByLoginNameAsync(string loginName)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.LoginName == loginName);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);
        }
        public async Task<List<string?>> GetAllAvatarUrlsAsync()
        {
            return await _context.Users
                .Select(x => x.Avatar)
                .ToListAsync();
        }
        public async Task<User?> GetByPhoneAsync(string phone)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Phone == phone);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
