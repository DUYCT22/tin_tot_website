using TinTot.Application.DTOs.Admin;
using TinTot.Application.Interfaces.Admin;
using TinTot.Application.Interfaces.Images;
using TinTot.Application.Interfaces.Notifications;

namespace TinTot.Application.Services.Admin;

public class AdminListingModerationService : IAdminListingModerationService
{
    private readonly IAdminListingModerationRepository _repository;
    private readonly IListingImageService _listingImageService;
    private readonly INotificationService _notificationService;

    public AdminListingModerationService(
        IAdminListingModerationRepository repository,
        IListingImageService listingImageService,
        INotificationService notificationService)
    {
        _repository = repository;
        _listingImageService = listingImageService;
        _notificationService = notificationService;
    }

    public async Task<AdminPendingListingsPageDto> GetPendingListingsAsync(int page = 1, int pageSize = 4)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 4;

        var (items, totalCount) = await _repository.GetPendingListingsAsync(page, pageSize);

        return new AdminPendingListingsPageDto
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            HasMore = page * pageSize < totalCount,
            Listings = items
        };
    }
    public async Task<AdminPendingListingsPageDto> GetApprovedListingsAsync(int page = 1, int pageSize = 4)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 4;

        var (items, totalCount) = await _repository.GetListingsByStatusAsync(status: 1, page, pageSize);

        return new AdminPendingListingsPageDto
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            HasMore = page * pageSize < totalCount,
            Listings = items
        };
    }
    public async Task ApproveListingAsync(int listingId)
    {
        var listing = await _repository.GetListingByIdWithUserAsync(listingId)
                      ?? throw new KeyNotFoundException("Bài đăng không tồn tại.");

        if (listing.Status != 0)
            throw new InvalidOperationException("Bài đăng này không ở trạng thái chờ duyệt.");

        listing.Status = 1;
        listing.UpdatedAt = DateTime.UtcNow;
        var createdAtText = listing.CreatedAt?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "không rõ thời gian";
        var listingTitle = string.IsNullOrWhiteSpace(listing.Title) ? $"#{listing.Id}" : listing.Title;
        if (listing.UserId.HasValue)
        {
            var message = $"✔️ Bài đăng {listingTitle}, đăng đăng lúc {createdAtText} đã được duyệt.";
            await _notificationService.CreateAndPublishAsync(listing.UserId.Value, null, null, message);
        }
        await _repository.UpdateAsync(listing);
        await _repository.SaveChangesAsync();
    }

    public async Task RejectListingAsync(int listingId)
    {
        var listing = await _repository.GetListingByIdWithUserAsync(listingId)
                      ?? throw new KeyNotFoundException("Bài đăng không tồn tại.");

        if (listing.Status != 0)
            throw new InvalidOperationException("Bài đăng này không ở trạng thái chờ duyệt.");
        await DeleteListingAndNotifyOwnerAsync(listing);
    }

    public async Task DeleteApprovedListingAsync(int listingId)
    {
        var listing = await _repository.GetListingByIdWithUserAsync(listingId)
                      ?? throw new KeyNotFoundException("Bài đăng không tồn tại.");

        if (listing.Status != 1)
            throw new InvalidOperationException("Bài đăng này không ở trạng thái đang hiển thị.");

        await DeleteListingAndNotifyOwnerAsync(listing);
    }

    private async Task DeleteListingAndNotifyOwnerAsync(TinTot.Domain.Entities.Listing listing)
    {

        var createdAtText = listing.CreatedAt?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "không rõ thời gian";
        var listingTitle = string.IsNullOrWhiteSpace(listing.Title) ? $"#{listing.Id}" : listing.Title;

        if (listing.UserId.HasValue)
        {
            var message = $"❌ Bài đăng {listingTitle}, đăng đăng lúc {createdAtText} không được duyệt do vi phạm điều khoản sử dụng hoặc chức ngôn từ/hình ảnh không phù hợp.";
            await _notificationService.CreateAndPublishAsync(listing.UserId.Value, null, null, message);
        }

        await _listingImageService.DeleteByListingIdAsync(listing.Id);
        await _repository.DeleteAsync(listing);
        await _repository.SaveChangesAsync();
    }
}
