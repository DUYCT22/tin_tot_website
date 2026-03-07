
namespace TinTot.Application.Interfaces.Listings
{
    public interface IInteractionRepository
    {
        Task<bool> ListingExistsAsync(int listingId);
        Task<int?> GetSellerIdByListingIdAsync(int listingId);
        Task<bool> SellerExistsAsync(int sellerId);

        Task<bool> IsFavoritedAsync(int userId, int listingId);
        Task AddFavoriteAsync(int userId, int listingId);
        Task RemoveFavoriteAsync(int userId, int listingId);

        Task<bool> IsFollowingAsync(int followerId, int sellerId);
        Task AddFollowAsync(int followerId, int sellerId);
        Task RemoveFollowAsync(int followerId, int sellerId);

        Task SaveChangesAsync();
    }
}
