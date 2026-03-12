using TinTot.Application.DTOs;
using TinTot.Application.Interfaces.Home;

namespace TinTot.Application.Services.Home
{
    public class HomeQueryService : IHomeQueryService
    {
        private readonly IHomeReadRepository _homeReadRepository;

        public HomeQueryService(IHomeReadRepository homeReadRepository)
        {
            _homeReadRepository = homeReadRepository;
        }

        public async Task<HomePageDto> GetHomePageDataAsync(int? currentUserId, int take = 6)
        {
            var banners = await _homeReadRepository.GetActiveBannersAsync();
            var categories = await _homeReadRepository.GetRootCategoriesAsync();
            var listings = await _homeReadRepository.GetLatestListingsAsync(currentUserId, take);
            var userIds = listings
                .Where(x => x.UserId.HasValue)
                .Select(x => x.UserId!.Value)
                .Distinct()
                .ToList();

            var ratingAvg = await _homeReadRepository.GetUserRatingAveragesAsync(userIds);

            return new HomePageDto
            {
                Banners = banners,
                Categories = categories,
                Listings = listings,
                UserRatingAvg = ratingAvg
            };
        }
        public async Task<AllListingsPageDto> GetAllListingsPageDataAsync(int? currentUserId, int? categoryId, string? keyword, string? sort, int page = 1, int pageSize = 12)
        {
            var normalizedSort = NormalizeSort(sort);
            var normalizedPage = page < 1 ? 1 : page;
            var normalizedKeyword = NormalizeKeyword(keyword);
            var normalizedPageSize = pageSize < 1 ? 12 : pageSize;

            var categories = await _homeReadRepository.GetAllCategoriesAsync();
            var (listings, totalCount) = await _homeReadRepository.GetFilteredListingsAsync(
                currentUserId,
                categoryId,
                normalizedKeyword,
                normalizedSort,
                normalizedPage,
                normalizedPageSize);

            var userIds = listings
                .Where(x => x.UserId.HasValue)
                .Select(x => x.UserId!.Value)
                .Distinct()
                .ToList();

            var ratingAvg = await _homeReadRepository.GetUserRatingAveragesAsync(userIds);

            return new AllListingsPageDto
            {
                Categories = categories,
                Listings = listings,
                UserRatingAvg = ratingAvg,
                SelectedCategoryId = categoryId,
                SelectedKeyword = normalizedKeyword,
                SelectedSort = normalizedSort,
                Page = normalizedPage,
                PageSize = normalizedPageSize,
                TotalCount = totalCount
            };
        }
        private static string? NormalizeKeyword(string? keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return null;
            }

            return keyword.Trim();
        }
        private static string NormalizeSort(string? sort)
            => sort?.ToLowerInvariant() switch
            {
                "oldest" => "oldest",
                "asc" => "asc",
                "desc" => "desc",
                _ => "newest"
            };
    }
}