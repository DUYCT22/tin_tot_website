namespace Tin_Tot_Website.Models
{
    public class Rating
    {
        public int Id { get; set; }

        public int UserId { get; set; }       // Người được đánh giá
        public int ReviewerId { get; set; }   // Người đánh giá

        public decimal? Score { get; set; }
        public string? Comment { get; set; }

        public DateTime? CreatedAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public User Reviewer { get; set; } = null!;
    }
}
