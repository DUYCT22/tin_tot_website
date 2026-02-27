using System.Reflection;

namespace TinTot.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? LoginName { get; set; }
        public string? Password { get; set; }
        public string? Avatar { get; set; }

        public int Role { get; set; }
        public bool Online { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Status { get; set; }

        // Navigation
        public ICollection<Listing> Listings { get; set; } = new List<Listing>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

        public ICollection<Follow> Following { get; set; } = new List<Follow>();
        public ICollection<Follow> Followers { get; set; } = new List<Follow>();

        public ICollection<Rating> ReceivedRatings { get; set; } = new List<Rating>();
        public ICollection<Rating> GivenRatings { get; set; } = new List<Rating>();
    }
}
