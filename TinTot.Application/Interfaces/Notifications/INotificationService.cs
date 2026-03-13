using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Application.DTOs.Notifications;

namespace TinTot.Application.Interfaces.Notifications
{
    public interface INotificationService
    {
        Task<IReadOnlyList<NotificationDto>> GetRecentAsync(int userId, int take = 20);
        Task<int> GetUnreadCountAsync(int userId);
        Task MarkAllAsReadAsync(int userId);
        Task CreateAndPublishAsync(int userId, int? relatedUserId, int? listingId, string message);
        Task<bool> CreateAndPublishUniqueAsync(int userId, int? relatedUserId, int? listingId, string message);
    }
}
