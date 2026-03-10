using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Tin_Tot_Website.Hubs
{
    [Authorize]
    public class MessageHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userIdRaw = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdRaw, out var userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            }

            await base.OnConnectedAsync();
        }
        public async Task NotifyTyping(int receiverId, bool isTyping)
        {
            var senderIdRaw = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(senderIdRaw, out var senderId) || receiverId <= 0)
            {
                return;
            }

            await Clients.Group($"user-{receiverId}").SendAsync("TypingStatusChanged", new
            {
                senderId,
                isTyping
            });
        }
    }
}
