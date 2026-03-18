using TinTot.Application.DTOs.Admin;

namespace TinTot.Application.Interfaces.Admin;

public interface IVisibleListingsExcelExporter
{
    byte[] Export(IReadOnlyList<AdminPendingListingItemDto> listings);
}
