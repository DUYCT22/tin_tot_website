using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Application.DTOs.Messages;
using TinTot.Domain.Entities;

namespace TinTot.Application.Interfaces.Messages
{
    public interface IMessageRepository
    {
        Task<List<ConversationPartnerDto>> GetConversationPartnersAsync(int userId);
        Task<List<ChatMessageDto>> GetConversationMessagesAsync(int currentUserId, int otherUserId, int take = 100);
        Task<bool> IsActiveUserAsync(int userId);
        Task<bool> ListingExistsAsync(int listingId);
        Task AddAsync(Message message);
        Task SaveChangesAsync();
    }
}
