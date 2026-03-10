using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Domain.Entities;

namespace TinTot.Application.Interfaces.Notifications
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetRecentByUserIdAsync(int userId, int take = 20);
        Task<int> GetUnreadCountAsync(int userId);
        Task AddAsync(Notification notification);
        Task MarkAllAsReadAsync(int userId);
        Task SaveChangesAsync();
    }
}
