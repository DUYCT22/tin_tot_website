using Microsoft.AspNetCore.SignalR;
using Tin_Tot_Website.Hubs;
using TinTot.Application.DTOs.Notifications;
using TinTot.Application.Interfaces.Notifications;

namespace Tin_Tot_Website.Services.Notifications
{
    public class SignalRNotificationRealtimePublisher : INotificationRealtimePublisher
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationRealtimePublisher(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task PublishAsync(int userId, NotificationDto notification, int unreadCount)
            => _hubContext.Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", new
            {
                notification.Id,
                notification.Message,
                notification.CreatedAt,
                notification.IsRead,
                unreadCount
            });
    }
}
