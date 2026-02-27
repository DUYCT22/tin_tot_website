namespace TinTot.Domain.Entities
{
    public class Image
    {
        public int Id { get; set; }

        public int ListingId { get; set; }
        public string? ImageUrl { get; set; }

        // Navigation
        public Listing Listing { get; set; } = null!;
    }
}
