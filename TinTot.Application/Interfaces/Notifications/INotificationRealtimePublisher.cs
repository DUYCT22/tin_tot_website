using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Application.DTOs.Notifications;

namespace TinTot.Application.Interfaces.Notifications
{
    public interface INotificationRealtimePublisher
    {
        Task PublishAsync(int userId, NotificationDto notification, int unreadCount);
    }
}
