using Microsoft.EntityFrameworkCore;
using TinTot.Application.Interfaces.Notifications;
using TinTot.Domain.Entities;
using TinTot.Infrastructure.Data;

namespace TinTot.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<Notification>> GetRecentByUserIdAsync(int userId, int take = 20)
            => _context.Notifications
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .ToListAsync();

        public Task<int> GetUnreadCountAsync(int userId)
            => _context.Notifications
                .AsNoTracking()
                .CountAsync(x => x.UserId == userId && !x.IsRead);

        public Task AddAsync(Notification notification) => _context.Notifications.AddAsync(notification).AsTask();

        public Task MarkAllAsReadAsync(int userId)
            => _context.Notifications
                .Where(x => x.UserId == userId && !x.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
