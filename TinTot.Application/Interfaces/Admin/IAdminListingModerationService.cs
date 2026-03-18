using TinTot.Application.DTOs.Admin;

namespace TinTot.Application.Interfaces.Admin;

public interface IAdminListingModerationService
{
    Task<AdminPendingListingsPageDto> GetPendingListingsAsync(int page = 1, int pageSize = 4);
    Task<AdminPendingListingsPageDto> GetApprovedListingsAsync(int page = 1, int pageSize = 4);
    Task ApproveListingAsync(int listingId);
    Task RejectListingAsync(int listingId);
    Task DeleteApprovedListingAsync(int listingId);
}
