
using TinTot.Domain.Entities;

namespace TinTot.Application.Interfaces.Images
{
    public interface IListingImageRepository
    {
        Task<Image?> GetByIdAsync(int id);
        Task<List<Image>> GetByListingIdAsync(int listingId);
        Task<bool> ListingExistsAsync(int listingId);
        Task<int> CountByListingIdAsync(int listingId);
        Task AddAsync(Image image);
        Task UpdateAsync(Image image);
        Task DeleteRangeAsync(IEnumerable<Image> images);
        Task SaveChangesAsync();
    }
}
