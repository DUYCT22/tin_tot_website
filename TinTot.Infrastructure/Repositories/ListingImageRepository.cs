using Microsoft.EntityFrameworkCore;
using TinTot.Application.Interfaces.Images;
using TinTot.Domain.Entities;
using TinTot.Infrastructure.Data;

namespace TinTot.Infrastructure.Repositories
{
    public class ListingImageRepository : IListingImageRepository
    {
        private readonly AppDbContext _context;

        public ListingImageRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Image?> GetByIdAsync(int id) => _context.Images.FirstOrDefaultAsync(x => x.Id == id);
        public Task<List<Image>> GetByListingIdAsync(int listingId) => _context.Images.Where(x => x.ListingId == listingId).ToListAsync();
        public Task<bool> ListingExistsAsync(int listingId) => _context.Listings.AnyAsync(x => x.Id == listingId);
        public Task<int> CountByListingIdAsync(int listingId) => _context.Images.CountAsync(x => x.ListingId == listingId);
        public async Task AddAsync(Image image) => await _context.Images.AddAsync(image);
        public Task UpdateAsync(Image image) { _context.Images.Update(image); return Task.CompletedTask; }
        public Task DeleteRangeAsync(IEnumerable<Image> images) { _context.Images.RemoveRange(images); return Task.CompletedTask; }
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
