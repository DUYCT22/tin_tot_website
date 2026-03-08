namespace Tin_Tot_Website.Models
{
    public class ProfilePageViewModel
    {
        public UserProfileSummaryViewModel User { get; set; } = new();
        public List<ProfileListingItemViewModel> ListingShowing { get; set; } = new();
        public List<ProfileListingItemViewModel> ListingPendingApproval { get; set; } = new();
        public List<ProfileListingItemViewModel> ListingSold { get; set; } = new();
        public List<ProfileFollowerItemViewModel> Followers { get; set; } = new();
        public List<ProfileRatingItemViewModel> Ratings { get; set; } = new();
        public List<ProfileCategoryOptionViewModel> Categories { get; set; } = new();
    }

    public class UserProfileSummaryViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int FollowersCount { get; set; }
        public int RatingsCount { get; set; }
    }

    public class ProfileListingItemViewModel
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Status { get; set; }
    }

    public class ProfileFollowerItemViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public DateTime? FollowedAt { get; set; }
    }

    public class ProfileRatingItemViewModel
    {
        public string ReviewerName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }

    public class ProfileCategoryOptionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentId { get; set; }
    }
}
