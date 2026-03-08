namespace Tin_Tot_Website.Models
{
    public class SavedListingViewModel
    {
        public int FavoriteId { get; set; }
        public string ListingKey { get; set; } = string.Empty;
        public string DetailUrl { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Location { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime? SavedAt { get; set; }
    }
}
