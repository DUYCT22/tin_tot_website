using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Application.Interfaces.Listings;

namespace TinTot.Application.Services.Listings
{
    public class InteractionService : IInteractionService
    {
        private readonly IInteractionRepository _repository;

        public InteractionService(IInteractionRepository repository)
        {
            _repository = repository;
        }

        public Task<bool> IsFavoritedAsync(int userId, int listingId) => _repository.IsFavoritedAsync(userId, listingId);

        public Task<bool> IsFollowingSellerAsync(int userId, int sellerId) => _repository.IsFollowingAsync(userId, sellerId);

        public async Task<string> AddFavoriteAsync(int userId, int listingId)
        {
            if (!await _repository.ListingExistsAsync(listingId))
                throw new KeyNotFoundException("Bài đăng không tồn tại.");

            if (await _repository.IsFavoritedAsync(userId, listingId))
                return "Bài đăng đã có trong danh sách yêu thích.";

            await _repository.AddFavoriteAsync(userId, listingId);
            await _repository.SaveChangesAsync();
            return "Đã thêm vào yêu thích.";
        }

        public async Task<string> RemoveFavoriteAsync(int userId, int listingId)
        {
            if (!await _repository.ListingExistsAsync(listingId))
                throw new KeyNotFoundException("Bài đăng không tồn tại.");

            if (!await _repository.IsFavoritedAsync(userId, listingId))
                return "Bài đăng chưa nằm trong yêu thích.";

            await _repository.RemoveFavoriteAsync(userId, listingId);
            await _repository.SaveChangesAsync();
            return "Đã hủy yêu thích.";
        }

        public async Task<string> FollowSellerAsync(int userId, int sellerId)
        {
            if (userId == sellerId)
                throw new InvalidOperationException("Bạn không thể theo dõi chính mình.");

            if (!await _repository.SellerExistsAsync(sellerId))
                throw new KeyNotFoundException("Người bán không tồn tại.");

            if (await _repository.IsFollowingAsync(userId, sellerId))
                return "Bạn đã theo dõi người bán này.";

            await _repository.AddFollowAsync(userId, sellerId);
            await _repository.SaveChangesAsync();
            return "Theo dõi người bán thành công.";
        }

        public async Task<string> UnfollowSellerAsync(int userId, int sellerId)
        {
            if (!await _repository.SellerExistsAsync(sellerId))
                throw new KeyNotFoundException("Người bán không tồn tại.");

            if (!await _repository.IsFollowingAsync(userId, sellerId))
                return "Bạn chưa theo dõi người bán này.";

            await _repository.RemoveFollowAsync(userId, sellerId);
            await _repository.SaveChangesAsync();
            return "Đã hủy theo dõi người bán.";
        }
    }
}
