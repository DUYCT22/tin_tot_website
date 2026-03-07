using TinTot.Application.DTOs;

namespace Tin_Tot_Website.Models
{
    public class ListingCardSectionViewModel
    {
        public IEnumerable<HomeListingDto> Listings { get; set; } = Enumerable.Empty<HomeListingDto>();
        public Dictionary<int, decimal> UserRatingAvg { get; set; } = new();
        public string EmptyMessage { get; set; } = "Chưa có bài đăng.";
    }
}
