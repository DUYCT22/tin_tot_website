using Microsoft.AspNetCore.SignalR;
using Tin_Tot_Website.Hubs;
using TinTot.Application.DTOs.Messages;
using TinTot.Application.Interfaces.Messages;

namespace Tin_Tot_Website.Services.Messages
{
    public class SignalRMessageRealtimePublisher : IMessageRealtimePublisher
    {
        private readonly IHubContext<MessageHub> _hubContext;

        public SignalRMessageRealtimePublisher(IHubContext<MessageHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task PublishToUsersAsync(int senderId, int receiverId, ChatMessageDto message)
            => Task.WhenAll(
                _hubContext.Clients.Group($"user-{senderId}").SendAsync("ReceiveMessage", message),
                _hubContext.Clients.Group($"user-{receiverId}").SendAsync("ReceiveMessage", message));
    }
}
