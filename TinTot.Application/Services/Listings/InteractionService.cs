using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Application.Interfaces.Listings;
using TinTot.Application.Interfaces.Notifications;

namespace TinTot.Application.Services.Listings
{
    public class InteractionService : IInteractionService
    {
        private readonly IInteractionRepository _repository;
        private readonly INotificationService _notificationService;

        public InteractionService(IInteractionRepository repository, INotificationService notificationService)
        {
            _repository = repository;
            _notificationService = notificationService;
        }

        public Task<bool> IsFavoritedAsync(int userId, int listingId) => _repository.IsFavoritedAsync(userId, listingId);

        public Task<bool> IsFollowingSellerAsync(int userId, int sellerId) => _repository.IsFollowingAsync(userId, sellerId);
        public async Task<bool> CanRateSellerAsync(int userId, int sellerId)
        {
            if (userId == sellerId)
            {
                return false;
            }

            if (!await _repository.SellerExistsAsync(sellerId))
            {
                return false;
            }

            if (await _repository.HasUserRatedSellerAsync(userId, sellerId))
            {
                return false;
            }

            return await _repository.SellerHasSoldListingAsync(sellerId);
        }


        public async Task<string> AddFavoriteAsync(int userId, int listingId)
        {
            if (!await _repository.ListingExistsAsync(listingId))
                throw new KeyNotFoundException("Bài đăng không tồn tại.");

            if (await _repository.IsFavoritedAsync(userId, listingId))
                return "Bài đăng đã có trong danh sách yêu thích.";

            await _repository.AddFavoriteAsync(userId, listingId);
            await _repository.SaveChangesAsync();
            var sellerId = await _repository.GetSellerIdByListingIdAsync(listingId);
            if (sellerId.HasValue && sellerId.Value != userId)
            {
                var actor = await _repository.GetUserDisplayNameAsync(userId) ?? "Một người dùng";
                var listingTitle = await _repository.GetListingTitleAsync(listingId) ?? "bài đăng của bạn";
                await _notificationService.CreateAndPublishUniqueAsync(
                    sellerId.Value,
                    userId,
                    listingId,
                    $"{actor} đã lưu tin \"{listingTitle}\" của bạn vào yêu thích.");
            }
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
            var actor = await _repository.GetUserDisplayNameAsync(userId) ?? "Một người dùng";
            await _notificationService.CreateAndPublishUniqueAsync(
                sellerId,
                userId,
                null,
                $"{actor} vừa theo dõi bạn.");
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
        public async Task<string> RateSellerAsync(int userId, int sellerId, decimal score, string? comment)
        {
            if (score < 1 || score > 5)
                throw new InvalidOperationException("Điểm đánh giá phải từ 1 đến 5.");

            if (string.IsNullOrWhiteSpace(comment))
                throw new InvalidOperationException("Vui lòng nhập nội dung đánh giá.");

            if (comment.Length > 1000)
                throw new InvalidOperationException("Nội dung đánh giá tối đa 1000 ký tự.");

            if (userId == sellerId)
                throw new InvalidOperationException("Bạn không thể tự đánh giá chính mình.");

            if (!await _repository.SellerExistsAsync(sellerId))
                throw new KeyNotFoundException("Người bán không tồn tại.");

            if (!await _repository.SellerHasSoldListingAsync(sellerId))
                throw new InvalidOperationException("Người bán chưa có bài đăng đã bán nên chưa thể nhận đánh giá.");

            await _repository.AddRatingAsync(sellerId, userId, score, comment.Trim());
            await _repository.SaveChangesAsync();

            var actor = await _repository.GetUserDisplayNameAsync(userId) ?? "Một người dùng";
            await _notificationService.CreateAndPublishAsync(
                sellerId,
                userId,
                null,
                $"{actor} vừa gửi đánh giá mới cho bạn.");

            return "Gửi đánh giá thành công.";
        }
    }
}
