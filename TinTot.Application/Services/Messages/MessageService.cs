using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Application.DTOs.Messages;
using TinTot.Application.Interfaces.Messages;
using TinTot.Domain.Entities;

namespace TinTot.Application.Services.Messages
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _repository;
        private readonly IMessageRealtimePublisher _realtimePublisher;

        public MessageService(IMessageRepository repository, IMessageRealtimePublisher realtimePublisher)
        {
            _repository = repository;
            _realtimePublisher = realtimePublisher;
        }

        public async Task<IReadOnlyList<ConversationPartnerDto>> GetConversationsAsync(int userId)
            => await _repository.GetConversationPartnersAsync(userId);

        public async Task<IReadOnlyList<ChatMessageDto>> GetHistoryAsync(int currentUserId, int otherUserId, int take = 100)
            => await _repository.GetConversationMessagesAsync(currentUserId, otherUserId, take);

        public async Task<ChatMessageDto> SendAsync(SendMessageRequestDto request)
        {
            if (request.SenderId == request.ReceiverId)
            {
                throw new InvalidOperationException("Bạn không thể nhắn cho chính mình.");
            }

            if (string.IsNullOrWhiteSpace(request.Content) && !request.ListingId.HasValue)
            {
                throw new InvalidOperationException("Không thể gửi tin nhắn rỗng.");
            }

            if (!await _repository.IsActiveUserAsync(request.ReceiverId))
            {
                throw new KeyNotFoundException("Người nhận không tồn tại hoặc đã bị khóa.");
            }

            if (request.ListingId.HasValue && !await _repository.ListingExistsAsync(request.ListingId.Value))
            {
                throw new KeyNotFoundException("Tin đăng đính kèm không tồn tại.");
            }

            var message = new Message
            {
                SenderId = request.SenderId,
                ReceiverId = request.ReceiverId,
                ListingId = request.ListingId,
                Content = request.Content.Trim(),
                SentAt = DateTime.UtcNow
            };

            await _repository.AddAsync(message);
            await _repository.SaveChangesAsync();

            var latest = (await _repository.GetConversationMessagesAsync(request.SenderId, request.ReceiverId, 1)).First();
            await _realtimePublisher.PublishToUsersAsync(request.SenderId, request.ReceiverId, latest);

            return latest;
        }
    }
}
