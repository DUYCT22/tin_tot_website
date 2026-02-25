namespace Tin_Tot_Website.Models
{
    public class Message
    {
        public int Id { get; set; }

        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public int? ListingId { get; set; }

        public string? Content { get; set; }
        public DateTime? SentAt { get; set; }

        // Navigation
        public User Sender { get; set; } = null!;
        public User Receiver { get; set; } = null!;
        public Listing? Listing { get; set; }
    }
}
