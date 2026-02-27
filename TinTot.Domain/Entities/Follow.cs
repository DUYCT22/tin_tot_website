namespace TinTot.Domain.Entities
{
    public class Follow
    {
        public int Id { get; set; }

        public int FollowerId { get; set; }
        public int SellerId { get; set; }

        public DateTime? CreatedAt { get; set; }

        // Navigation
        public User Follower { get; set; } = null!;
        public User Seller { get; set; } = null!;
    }
}
