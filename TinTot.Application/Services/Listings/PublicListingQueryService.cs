using TinTot.Application.Common;
using TinTot.Application.DTOs.Listing;
using TinTot.Application.Interfaces.Listings;

namespace TinTot.Application.Services.Listings
{
    public class PublicListingQueryService : IPublicListingQueryService
    {
        private readonly IPublicListingReadRepository _repository;

        public PublicListingQueryService(IPublicListingReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<ListingDetailDto?> GetListingDetailAsync(int listingId)
        {
            var detail = await _repository.GetListingDetailAsync(listingId);
            if (detail is null)
            {
                return null;
            }

            detail.Slug = SlugHelper.ToSlug(detail.Title);

            if (detail.RelatedListings.Count > 0)
            {
                var userIds = detail.RelatedListings
                    .Where(x => x.UserId.HasValue)
                    .Select(x => x.UserId!.Value)
                    .Distinct()
                    .ToList();

                var ratingMap = await _repository.GetUserRatingAveragesAsync(userIds);
                foreach (var item in detail.RelatedListings)
                {
                    if (item.UserId.HasValue && ratingMap.TryGetValue(item.UserId.Value, out var avg))
                    {
                        item.UserRatingAverage = avg;
                    }
                }
            }

            return detail;
        }

        public async Task<SellerProfileDto?> GetSellerProfileAsync(int sellerId, int take = 12)
        {
            var profile = await _repository.GetSellerProfileAsync(sellerId, take);
            if (profile is null)
            {
                return null;
            }

            profile.Slug = SlugHelper.ToSlug(profile.SellerName);
            var userIds = profile.ActiveListings
                .Where(x => x.UserId.HasValue)
                .Select(x => x.UserId!.Value)
                .Distinct()
                .ToList();
            profile.UserRatingAvg = await _repository.GetUserRatingAveragesAsync(userIds);

            return profile;
        }
    }
}
