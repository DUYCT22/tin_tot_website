

namespace Tin_Tot_Website.Models
{
    public class Listing
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int CategoryId { get; set; }

        public string? Title { get; set; }
        public string? Description { get; set; }

        public decimal? Price { get; set; }
        public string? Location { get; set; }

        public int Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public Category Category { get; set; } = null!;

        public ICollection<Image> Images { get; set; } = new List<Image>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
