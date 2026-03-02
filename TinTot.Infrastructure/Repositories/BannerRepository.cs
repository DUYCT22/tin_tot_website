using Microsoft.EntityFrameworkCore;
using TinTot.Application.Interfaces.Banners;
using TinTot.Domain.Entities;
using TinTot.Infrastructure.Data;

namespace TinTot.Infrastructure.Repositories
{
    public class BannerRepository : IBannerRepository
    {
        private readonly AppDbContext _context;

        public BannerRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Banner?> GetByIdAsync(int id) => _context.Banners.FirstOrDefaultAsync(x => x.Id == id);
        public async Task AddAsync(Banner banner) => await _context.Banners.AddAsync(banner);
        public Task UpdateAsync(Banner banner) { _context.Banners.Update(banner); return Task.CompletedTask; }
        public Task DeleteAsync(Banner banner) { _context.Banners.Remove(banner); return Task.CompletedTask; }
        public Task SaveChangesAsync() => _context.SaveChangesAsync();

    }
}
