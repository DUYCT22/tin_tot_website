using Microsoft.EntityFrameworkCore;
using TinTot.Application.DTOs;
using TinTot.Application.Interfaces.Home;
using TinTot.Infrastructure.Data;

namespace TinTot.Infrastructure.Repositories.Home
{
    public class HomeReadRepository : IHomeReadRepository
    {
        private readonly AppDbContext _context;

        public HomeReadRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BannerDto>> GetActiveBannersAsync()
        {
            return await _context.Banners
                .AsNoTracking()
                .Where(x => x.Status)
                .OrderBy(x => x.Orders)
                .Select(x => new BannerDto
                {
                    Id = x.Id,
                    Link = x.Link,
                    Image = x.Image,
                    Status = x.Status,
                    Orders = x.Orders,
                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy
                })
                .ToListAsync();
        }

        public async Task<List<CategoryDto>> GetRootCategoriesAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(x => x.ParentId == null)
                .OrderBy(x => x.Name)
                .Select(x => new CategoryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    ParentId = x.ParentId,
                    Image = x.Image,
                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy
                })
                .ToListAsync();
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new CategoryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    ParentId = x.ParentId,
                    Image = x.Image,
                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy
                })
                .ToListAsync();
        }

        public async Task<List<HomeListingDto>> GetLatestListingsAsync(int? excludedUserId, int take)
        {
            var query = BuildPublicListingQuery(excludedUserId);

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .Select(ToListingDto())
                .ToListAsync();
        }

        public async Task<(List<HomeListingDto> Listings, int TotalCount)> GetFilteredListingsAsync(int? excludedUserId, int? categoryId, string? keyword, string? sort, int page, int pageSize)
        {
            var query = BuildPublicListingQuery(excludedUserId);

            if (categoryId.HasValue)
            {
                var selectedCategoryId = categoryId.Value;
                var categoryIds = await _context.Categories
                    .AsNoTracking()
                    .Where(x => x.Id == selectedCategoryId || x.ParentId == selectedCategoryId)
                    .Select(x => x.Id)
                    .ToListAsync();

                if (categoryIds.Count == 0)
                {
                    categoryIds.Add(selectedCategoryId);
                }

                query = query.Where(x => categoryIds.Contains(x.CategoryId));
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var normalizedKeyword = keyword.Trim().ToLower();
                query = query.Where(x => x.Title != null && x.Title.ToLower().Contains(normalizedKeyword));
            }
            query = (sort ?? "newest").ToLowerInvariant() switch
            {
                "oldest" => query.OrderBy(x => x.CreatedAt),
                "asc" => query.OrderBy(x => x.Price),
                "desc" => query.OrderByDescending(x => x.Price),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var listings = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToListingDto())
                .ToListAsync();

            return (listings, totalCount);
        }

        public async Task<Dictionary<int, decimal>> GetUserRatingAveragesAsync(IEnumerable<int> userIds)
        {
            var ids = userIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return new Dictionary<int, decimal>();
            }

            return await _context.Ratings
                .AsNoTracking()
                .Where(x => ids.Contains(x.UserId) && x.Score.HasValue)
                .GroupBy(x => x.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.Average(x => x.Score ?? 0));
        }
        private IQueryable<TinTot.Domain.Entities.Listing> BuildPublicListingQuery(int? excludedUserId)
        {
            var query = _context.Listings
                .AsNoTracking()
                .Include(x => x.Images)
                .Include(x => x.User)
                .Where(x => x.Status == 1);

            if (excludedUserId.HasValue)
            {
                query = query.Where(x => x.UserId != excludedUserId.Value);
            }

            return query;
        }

        private static System.Linq.Expressions.Expression<Func<TinTot.Domain.Entities.Listing, HomeListingDto>> ToListingDto()
            => x => new HomeListingDto
            {
                Id = x.Id,
                UserId = x.UserId,
                Title = x.Title,
                Price = x.Price,
                CreatedAt = x.CreatedAt,
                UserAvatar = x.User != null ? x.User.Avatar : null,
                FirstImageUrl = x.Images.OrderBy(i => i.Id).Select(i => i.ImageUrl).FirstOrDefault()
            };
    }
}