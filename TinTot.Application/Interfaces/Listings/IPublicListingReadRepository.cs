using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Application.DTOs.Listing;

namespace TinTot.Application.Interfaces.Listings
{
    public interface IPublicListingReadRepository
    {
        Task<ListingDetailDto?> GetListingDetailAsync(int listingId);
        Task<Dictionary<int, decimal>> GetUserRatingAveragesAsync(IEnumerable<int> userIds);
        Task<SellerProfileDto?> GetSellerProfileAsync(int sellerId, int take = 12);
    }
}
