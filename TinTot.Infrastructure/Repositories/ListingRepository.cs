using Microsoft.EntityFrameworkCore;
using TinTot.Application.Interfaces.Listings;
using TinTot.Domain.Entities;
using TinTot.Infrastructure.Data;

namespace TinTot.Infrastructure.Repositories
{
    public class ListingRepository : IListingRepository
    {
        private readonly AppDbContext _context;

        public ListingRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Listing?> GetByIdAsync(int id) => _context.Listings.FirstOrDefaultAsync(x => x.Id == id);
        public Task<bool> UserExistsAsync(int? userId) => _context.Users.AnyAsync(x => x.Id == userId);
        public Task<bool> CategoryExistsAsync(int categoryId) => _context.Categories.AnyAsync(x => x.Id == categoryId);
        public async Task AddAsync(Listing listing) => await _context.Listings.AddAsync(listing);
        public Task UpdateAsync(Listing listing) { _context.Listings.Update(listing); return Task.CompletedTask; }
        public Task DeleteAsync(Listing listing) { _context.Listings.Remove(listing); return Task.CompletedTask; }
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
