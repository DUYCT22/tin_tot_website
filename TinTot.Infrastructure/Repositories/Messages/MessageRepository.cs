using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TinTot.Application.DTOs.Messages;
using TinTot.Application.Interfaces.Messages;
using TinTot.Domain.Entities;
using TinTot.Infrastructure.Data;

namespace TinTot.Infrastructure.Repositories.Messages
{
    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _context;

        public MessageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ConversationPartnerDto>> GetConversationPartnersAsync(int userId)
        {
            var latestByPartner = await _context.Messages
                .AsNoTracking()
                .Where(x => x.SenderId == userId || x.ReceiverId == userId)
                .GroupBy(x => x.SenderId == userId ? x.ReceiverId : x.SenderId)
                .Select(g => g.OrderByDescending(m => m.SentAt).ThenByDescending(m => m.Id).First())
                .ToListAsync();

            var partnerIds = latestByPartner
                .Select(x => x.SenderId == userId ? x.ReceiverId : x.SenderId)
                .Distinct()
                .ToList();

            var users = await _context.Users
                .AsNoTracking()
                .Where(x => partnerIds.Contains(x.Id) && x.Status)
                .ToDictionaryAsync(x => x.Id);

            return latestByPartner
                .Where(m => users.ContainsKey(m.SenderId == userId ? m.ReceiverId : m.SenderId))
                .OrderByDescending(m => m.SentAt)
                .Select(m =>
                {
                    var partnerId = m.SenderId == userId ? m.ReceiverId : m.SenderId;
                    var partner = users[partnerId];
                    return new ConversationPartnerDto
                    {
                        UserId = partnerId,
                        DisplayName = partner.FullName ?? partner.LoginName ?? "Người dùng",
                        Avatar = partner.Avatar ?? string.Empty,
                        LastMessage = string.IsNullOrWhiteSpace(m.Content) ? "[Tin nhắn đính kèm]" : m.Content!,
                        LastSentAt = m.SentAt
                    };
                })
                .ToList();
        }

        public Task<List<ChatMessageDto>> GetConversationMessagesAsync(int currentUserId, int otherUserId, int take = 100)
            => _context.Messages
                .AsNoTracking()
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == otherUserId)
                         || (m.SenderId == otherUserId && m.ReceiverId == currentUserId))
                .OrderByDescending(m => m.SentAt)
                .ThenByDescending(m => m.Id)
                .Take(take)
                .OrderBy(m => m.SentAt)
                .ThenBy(m => m.Id)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    ListingId = m.ListingId,
                    Content = m.Content ?? string.Empty,
                    SentAt = m.SentAt,
                    SenderName = m.Sender.FullName ?? m.Sender.LoginName ?? "Người dùng",
                    ReceiverName = m.Receiver.FullName ?? m.Receiver.LoginName ?? "Người dùng",
                    ListingTitle = m.Listing != null ? m.Listing.Title : null,
                    ListingImageUrl = m.Listing != null ? m.Listing.Images.OrderBy(i => i.Id).Select(i => i.ImageUrl).FirstOrDefault() : null
                })
                .ToListAsync();

        public Task<bool> IsActiveUserAsync(int userId)
            => _context.Users.AsNoTracking().AnyAsync(x => x.Id == userId && x.Status);

        public Task<bool> ListingExistsAsync(int listingId)
            => _context.Listings.AsNoTracking().AnyAsync(x => x.Id == listingId);

        public Task AddAsync(Message message) => _context.Messages.AddAsync(message).AsTask();

        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
