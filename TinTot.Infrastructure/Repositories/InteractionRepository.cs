using Microsoft.EntityFrameworkCore;
using TinTot.Application.Interfaces.Listings;
using TinTot.Domain.Entities;
using TinTot.Infrastructure.Data;

namespace TinTot.Infrastructure.Repositories
{
    public class InteractionRepository : IInteractionRepository
    {
        private readonly AppDbContext _context;

        public InteractionRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<bool> ListingExistsAsync(int listingId) => _context.Listings.AnyAsync(x => x.Id == listingId);

        public Task<int?> GetSellerIdByListingIdAsync(int listingId) => _context.Listings.Where(x => x.Id == listingId).Select(x => x.UserId).FirstOrDefaultAsync();
        public Task<string?> GetUserDisplayNameAsync(int userId) => _context.Users
            .Where(x => x.Id == userId)
            .Select(x => x.FullName ?? x.LoginName)
            .FirstOrDefaultAsync();

        public Task<string?> GetListingTitleAsync(int listingId) => _context.Listings
            .Where(x => x.Id == listingId)
            .Select(x => x.Title)
            .FirstOrDefaultAsync();
        public Task<bool> SellerExistsAsync(int sellerId) => _context.Users.AnyAsync(x => x.Id == sellerId && x.Status);
        public Task<bool> SellerHasSoldListingAsync(int sellerId) => _context.Listings.AnyAsync(x => x.UserId == sellerId && x.Status == 2);
        public Task<bool> HasUserRatedSellerAsync(int reviewerId, int sellerId) => _context.Ratings.AnyAsync(x => x.ReviewerId == reviewerId && x.UserId == sellerId);
        public Task<bool> IsFavoritedAsync(int userId, int listingId) => _context.Favorites.AnyAsync(x => x.UserId == userId && x.ListingId == listingId);

        public async Task AddFavoriteAsync(int userId, int listingId)
        {
            await _context.Favorites.AddAsync(new Favorite
            {
                UserId = userId,
                ListingId = listingId,
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task RemoveFavoriteAsync(int userId, int listingId)
        {
            var favorite = await _context.Favorites.FirstOrDefaultAsync(x => x.UserId == userId && x.ListingId == listingId);
            if (favorite is not null)
            {
                _context.Favorites.Remove(favorite);
            }
        }

        public Task<bool> IsFollowingAsync(int followerId, int sellerId) => _context.Follows.AnyAsync(x => x.FollowerId == followerId && x.SellerId == sellerId);

        public async Task AddFollowAsync(int followerId, int sellerId)
        {
            await _context.Follows.AddAsync(new Follow
            {
                FollowerId = followerId,
                SellerId = sellerId,
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task RemoveFollowAsync(int followerId, int sellerId)
        {
            var follow = await _context.Follows.FirstOrDefaultAsync(x => x.FollowerId == followerId && x.SellerId == sellerId);
            if (follow is not null)
            {
                _context.Follows.Remove(follow);
            }
        }
        public async Task AddRatingAsync(int userId, int reviewerId, decimal score, string? comment)
        {
            await _context.Ratings.AddAsync(new Rating
            {
                UserId = userId,
                ReviewerId = reviewerId,
                Score = score,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            });
        }
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
