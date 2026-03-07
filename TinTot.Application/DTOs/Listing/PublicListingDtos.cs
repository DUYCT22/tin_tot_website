using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinTot.Application.DTOs.Listing
{
    public class ListingDetailDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Location { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CategoryName { get; set; }
        public string? UserFullName { get; set; }
        public string? UserAvatar { get; set; }
        public string? UserPhone { get; set; }
        public decimal UserRatingAverage { get; set; }
        public int UserRatingCount { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public List<HomeListingDto> RelatedListings { get; set; } = new();
    }

    public class SellerProfileDto
    {
        public int SellerId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string? SellerName { get; set; }
        public string? SellerAvatar { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Status { get; set; }
        public int FollowerCount { get; set; }
        public decimal RatingAverage { get; set; }
        public int RatingCount { get; set; }
        public int ActiveListingCount { get; set; }
        public int SoldListingCount { get; set; }
        public List<HomeListingDto> ActiveListings { get; set; } = new();
        public Dictionary<int, decimal> UserRatingAvg { get; set; } = new();
    }
}
