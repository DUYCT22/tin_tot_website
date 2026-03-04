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
    }
}