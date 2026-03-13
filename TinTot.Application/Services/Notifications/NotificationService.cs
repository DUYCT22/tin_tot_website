using TinTot.Application.DTOs.Notifications;
using TinTot.Application.Interfaces.Notifications;
using TinTot.Domain.Entities;

namespace TinTot.Application.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly INotificationRealtimePublisher _publisher;

        public NotificationService(INotificationRepository repository, INotificationRealtimePublisher publisher)
        {
            _repository = repository;
            _publisher = publisher;
        }

        public async Task<IReadOnlyList<NotificationDto>> GetRecentAsync(int userId, int take = 20)
        {
            var notifications = await _repository.GetRecentByUserIdAsync(userId, take);
            return notifications.Select(Map).ToList();
        }

        public Task<int> GetUnreadCountAsync(int userId) => _repository.GetUnreadCountAsync(userId);

        public async Task MarkAllAsReadAsync(int userId)
        {
            await _repository.MarkAllAsReadAsync(userId);
            await _repository.SaveChangesAsync();
        }

        public async Task CreateAndPublishAsync(int userId, int? relatedUserId, int? listingId, string message)
        {
            await CreateAndPublishInternalAsync(userId, relatedUserId, listingId, message);
        }

        public async Task<bool> CreateAndPublishUniqueAsync(int userId, int? relatedUserId, int? listingId, string message)
        {
            if (await _repository.ExistsAsync(userId, relatedUserId, listingId, message))
            {
                return false;
            }

            await CreateAndPublishInternalAsync(userId, relatedUserId, listingId, message);
            return true;
        }

        private async Task CreateAndPublishInternalAsync(int userId, int? relatedUserId, int? listingId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                RelatedUserId = relatedUserId,
                ListingId = listingId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(notification);
            await _repository.SaveChangesAsync();

            var dto = Map(notification);
            var unreadCount = await _repository.GetUnreadCountAsync(userId);
            await _publisher.PublishAsync(userId, dto, unreadCount);
        }

        private static NotificationDto Map(Notification n) => new()
        {
            Id = n.Id,
            UserId = n.UserId,
            RelatedUserId = n.RelatedUserId,
            ListingId = n.ListingId,
            Message = n.Message ?? string.Empty,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        };
    }
}
