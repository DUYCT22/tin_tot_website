using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Application.DTOs.Messages;

namespace TinTot.Application.Interfaces.Messages
{
    public interface IMessageService
    {
        Task<IReadOnlyList<ConversationPartnerDto>> GetConversationsAsync(int userId);
        Task<IReadOnlyList<ChatMessageDto>> GetHistoryAsync(int currentUserId, int otherUserId, int take = 100);
        Task<ChatMessageDto> SendAsync(SendMessageRequestDto request);
    }
}
