using TinTot.Application.DTOs.Admin;
using TinTot.Domain.Entities;

namespace TinTot.Application.Interfaces.Admin;

public interface IAdminListingModerationRepository
{
    Task<(List<AdminPendingListingItemDto> Items, int TotalCount)> GetPendingListingsAsync(int page, int pageSize);
    Task<(List<AdminPendingListingItemDto> Items, int TotalCount)> GetListingsByStatusAsync(int status, int page, int pageSize);
    Task<List<AdminPendingListingItemDto>> GetListingsByStatusForExportAsync(int status, IReadOnlyCollection<int>? listingIds = null);
    Task<Listing?> GetListingByIdWithUserAsync(int listingId);
    Task UpdateAsync(Listing listing);
    Task DeleteAsync(Listing listing);
    Task SaveChangesAsync();
}
