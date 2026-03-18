namespace TinTot.Application.DTOs.Admin;

public class AdminPendingListingItemDto
{
    public int ListingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public int? UserId { get; set; }
    public string PosterName { get; set; } = string.Empty;
    public string PosterKey { get; set; } = string.Empty;
    public string ListingKey { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
}

public class AdminPendingListingsPageDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasMore { get; set; }
    public List<AdminPendingListingItemDto> Listings { get; set; } = new();
}
