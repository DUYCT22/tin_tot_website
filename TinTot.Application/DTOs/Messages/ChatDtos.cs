namespace TinTot.Application.DTOs.Messages
{
    public class ConversationPartnerDto
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime? LastSentAt { get; set; }
    }

    public class ChatMessageDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public int? ListingId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime? SentAt { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string? ListingTitle { get; set; }
        public string? ListingImageUrl { get; set; }
    }

    public class SendMessageRequestDto
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public int? ListingId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
