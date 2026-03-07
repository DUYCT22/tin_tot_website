using TinTot.Application.DTOs.Listing;

namespace Tin_Tot_Website.Models
{
    public class SellerProfilePageViewModel
    {
        public SellerProfileDto Seller { get; set; } = new();
        public string SellerKey { get; set; } = string.Empty;
        public ListingCardSectionViewModel ActiveListingsSection { get; set; } = new();
    }
}
