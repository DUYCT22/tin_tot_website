using TinTot.Domain.Entities;

namespace TinTot.Application.Interfaces.Listings
{
    public interface IListingRepository
    {
        Task<Listing?> GetByIdAsync(int id);
        Task<bool> UserExistsAsync(int? userId);
        Task<bool> CategoryExistsAsync(int categoryId);
        Task AddAsync(Listing listing);
        Task UpdateAsync(Listing listing);
        Task DeleteAsync(Listing listing);
        Task SaveChangesAsync();
    }
}
