namespace Tin_Tot_Website.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int? RelatedUserId { get; set; }
        public int? ListingId { get; set; }

        public string? Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public User? RelatedUser { get; set; }
        public Listing? Listing { get; set; }
    }
}
