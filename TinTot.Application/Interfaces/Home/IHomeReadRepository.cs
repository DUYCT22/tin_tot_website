using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Application.DTOs;

namespace TinTot.Application.Interfaces.Home
{
    public interface IHomeReadRepository
    {
        Task<List<HomeListingDto>> GetLatestListingsAsync(int? excludedUserId, int take);
        Task<(List<HomeListingDto> Listings, int TotalCount)> GetFilteredListingsAsync(int? excludedUserId, int? categoryId, string? keyword, string? sort, int page, int pageSize);
        Task<List<CategoryDto>> GetRootCategoriesAsync();
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task<List<BannerDto>> GetActiveBannersAsync();
        Task<Dictionary<int, decimal>> GetUserRatingAveragesAsync(IEnumerable<int> userIds);
    }
}
