using Microsoft.EntityFrameworkCore;
using TinTot.Application.DTOs;
using TinTot.Application.DTOs.Listing;
using TinTot.Application.Interfaces.Listings;
using TinTot.Infrastructure.Data;

namespace TinTot.Infrastructure.Repositories
{
    public class PublicListingReadRepository : IPublicListingReadRepository
    {
        private readonly AppDbContext _context;

        public PublicListingReadRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ListingDetailDto?> GetListingDetailAsync(int listingId)
        {
            var listing = await _context.Listings
                .AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Category)
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.Id == listingId);

            if (listing is null)
            {
                return null;
            }

            var ratingAgg = await _context.Ratings
                .AsNoTracking()
                .Where(x => x.UserId == listing.UserId && x.Score.HasValue)
                .GroupBy(x => x.UserId)
                .Select(g => new
                {
                    Avg = g.Average(x => x.Score ?? 0),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync();
            var sellerRatings = await _context.Ratings
                .AsNoTracking()
                .Include(x => x.Reviewer)
                .Where(x => x.UserId == listing.UserId && x.Score.HasValue)
                .OrderByDescending(x => x.CreatedAt)
                .Take(8)
                .Select(x => new ListingSellerRatingDto
                {
                    ReviewerName = x.Reviewer.FullName ?? x.Reviewer.LoginName ?? "Người dùng",
                    Score = x.Score ?? 0,
                    Comment = x.Comment ?? string.Empty,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
            var parentCategoryId = await _context.Categories
                .AsNoTracking()
                .Where(c => c.Id == listing.CategoryId)
                .Select(c => c.ParentId ?? c.Id)
                .FirstOrDefaultAsync();

            var siblingCategoryIds = await _context.Categories
                .AsNoTracking()
                .Where(c => c.Id == parentCategoryId || c.ParentId == parentCategoryId)
                .Select(c => c.Id)
                .ToListAsync();

            var relatedListings = await _context.Listings
                .AsNoTracking()
                .Include(x => x.Images)
                .Include(x => x.User)
                .Where(x => x.Id != listing.Id
                            && siblingCategoryIds.Contains(x.CategoryId)
                            && (x.Status == 0 || x.Status == 1))
                .OrderByDescending(x => x.CreatedAt)
                .Take(6)
                .Select(x => new HomeListingDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Title = x.Title,
                    Price = x.Price,
                    CreatedAt = x.CreatedAt,
                    UserAvatar = x.User != null ? x.User.Avatar : null,
                    FirstImageUrl = x.Images.OrderBy(i => i.Id).Select(i => i.ImageUrl).FirstOrDefault()
                })
                .ToListAsync();

            return new ListingDetailDto
            {
                Id = listing.Id,
                UserId = listing.UserId,
                Title = listing.Title,
                Description = listing.Description,
                Price = listing.Price,
                Location = listing.Location,
                CreatedAt = listing.CreatedAt,
                CategoryName = listing.Category?.Name,
                UserFullName = listing.User?.FullName,
                UserAvatar = listing.User?.Avatar,
                UserPhone = listing.User?.Phone,
                UserRatingAverage = ratingAgg?.Avg ?? 0,
                UserRatingCount = ratingAgg?.Count ?? 0,
                SellerRatings = sellerRatings,
                ImageUrls = listing.Images.OrderBy(x => x.Id).Select(x => x.ImageUrl ?? string.Empty).Where(x => !string.IsNullOrWhiteSpace(x)).ToList(),
                RelatedListings = relatedListings
            };
        }

        public async Task<Dictionary<int, decimal>> GetUserRatingAveragesAsync(IEnumerable<int> userIds)
        {
            var ids = userIds.Distinct().ToList();
            if (!ids.Any())
            {
                return new Dictionary<int, decimal>();
            }

            return await _context.Ratings
                .AsNoTracking()
                .Where(x => ids.Contains(x.UserId) && x.Score.HasValue)
                .GroupBy(x => x.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.Average(x => x.Score ?? 0));
        }

        public async Task<SellerProfileDto?> GetSellerProfileAsync(int sellerId, int take = 12)
        {
            var seller = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == sellerId);

            if (seller is null)
            {
                return null;
            }

            var activeListings = await _context.Listings
                .AsNoTracking()
                .Include(x => x.Images)
                .Where(x => x.UserId == sellerId && (x.Status == 0 || x.Status == 1))
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .Select(x => new HomeListingDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Title = x.Title,
                    Price = x.Price,
                    CreatedAt = x.CreatedAt,
                    UserAvatar = seller.Avatar,
                    FirstImageUrl = x.Images.OrderBy(i => i.Id).Select(i => i.ImageUrl).FirstOrDefault()
                })
                .ToListAsync();

            var soldCount = await _context.Listings.AsNoTracking().CountAsync(x => x.UserId == sellerId && x.Status == 2);
            var followCount = await _context.Follows.AsNoTracking().CountAsync(x => x.SellerId == sellerId);

            var ratingAgg = await _context.Ratings
                .AsNoTracking()
                .Where(x => x.UserId == sellerId && x.Score.HasValue)
                .GroupBy(x => x.UserId)
                .Select(g => new
                {
                    Avg = g.Average(x => x.Score ?? 0),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync();

            return new SellerProfileDto
            {
                SellerId = seller.Id,
                SellerName = seller.FullName,
                SellerAvatar = seller.Avatar,
                CreatedAt = seller.CreatedAt,
                Status = seller.Status,
                FollowerCount = followCount,
                RatingAverage = ratingAgg?.Avg ?? 0,
                RatingCount = ratingAgg?.Count ?? 0,
                ActiveListingCount = activeListings.Count,
                SoldListingCount = soldCount,
                ActiveListings = activeListings
            };
        }
    }
}
