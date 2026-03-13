
namespace TinTot.Application.Interfaces.Listings
{
    public interface IInteractionService
    {
        Task<bool> IsFavoritedAsync(int userId, int listingId);
        Task<bool> IsFollowingSellerAsync(int userId, int sellerId);
        Task<bool> CanRateSellerAsync(int userId, int sellerId);
        Task<string> AddFavoriteAsync(int userId, int listingId);
        Task<string> RemoveFavoriteAsync(int userId, int listingId);
        Task<string> FollowSellerAsync(int userId, int sellerId);
        Task<string> UnfollowSellerAsync(int userId, int sellerId);
        Task<string> RateSellerAsync(int userId, int sellerId, decimal score, string? comment);
    }
}
