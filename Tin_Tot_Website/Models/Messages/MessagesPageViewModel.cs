namespace Tin_Tot_Website.Models.Messages
{
    public class MessagesPageViewModel
    {
        public string? ReceiverKey { get; set; }
        public string? ListingKey { get; set; }
    }

    public class SendMessageRequestModel
    {
        public string ReceiverKey { get; set; } = string.Empty;
        public string? ListingKey { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
