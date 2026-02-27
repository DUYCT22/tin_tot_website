using System.Reflection;
namespace TinTot.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }

        public string? Name { get; set; }
        public int? ParentId { get; set; }
        public string? Image { get; set; }

        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        // Navigation
        public Category? Parent { get; set; }
        public ICollection<Category> Children { get; set; } = new List<Category>();

        public User? CreatedByUser { get; set; }
        public User? UpdatedByUser { get; set; }

        public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    }
}
