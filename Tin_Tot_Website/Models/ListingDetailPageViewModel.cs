using TinTot.Application.DTOs.Listing;

namespace Tin_Tot_Website.Models
{
    public class ListingDetailPageViewModel
    {
        public ListingDetailDto Listing { get; set; } = new();
        public string ListingKey { get; set; } = string.Empty;
        public string SellerKey { get; set; } = string.Empty;
        public ListingCardSectionViewModel RelatedSection { get; set; } = new();
    }
}
