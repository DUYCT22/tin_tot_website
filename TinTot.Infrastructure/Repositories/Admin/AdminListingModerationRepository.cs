using Microsoft.EntityFrameworkCore;
using TinTot.Application.DTOs.Admin;
using TinTot.Application.Interfaces.Admin;
using TinTot.Domain.Entities;
using TinTot.Infrastructure.Data;

namespace TinTot.Infrastructure.Repositories.Admin;

public class AdminListingModerationRepository : IAdminListingModerationRepository
{
    private readonly AppDbContext _context;

    public AdminListingModerationRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<(List<AdminPendingListingItemDto> Items, int TotalCount)> GetPendingListingsAsync(int page, int pageSize)
        => GetListingsByStatusAsync(status: 0, page, pageSize);

    public async Task<(List<AdminPendingListingItemDto> Items, int TotalCount)> GetListingsByStatusAsync(int status, int page, int pageSize)
    {
        var query = _context.Listings
           .AsNoTracking()
            .Where(x => x.Status == status);

        var totalCount = await query.CountAsync();

        var listings = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AdminPendingListingItemDto
            {
                ListingId = x.Id,
                Title = x.Title ?? string.Empty,
                Description = x.Description ?? string.Empty,
                Location = x.Location ?? string.Empty,
                CategoryName = x.Category.Name ?? string.Empty,
                CreatedAt = x.CreatedAt,
                UserId = x.UserId,
                PosterName = x.User != null
                    ? (x.User.FullName ?? x.User.LoginName ?? $"User {x.User.Id}")
                    : "Không rõ",
                ImageUrls = x.Images
                    .OrderBy(img => img.Id)
                    .Select(img => img.ImageUrl ?? string.Empty)
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Take(4)
                    .ToList()
            })
            .ToListAsync();

        return (listings, totalCount);
    }
    public async Task<List<AdminPendingListingItemDto>> GetListingsByStatusForExportAsync(int status, IReadOnlyCollection<int>? listingIds = null)
    {
        var query = _context.Listings
            .AsNoTracking()
            .Where(x => x.Status == status);

        if (listingIds is { Count: > 0 })
        {
            query = query.Where(x => listingIds.Contains(x.Id));
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AdminPendingListingItemDto
            {
                ListingId = x.Id,
                Title = x.Title ?? string.Empty,
                Description = x.Description ?? string.Empty,
                Location = x.Location ?? string.Empty,
                CategoryName = x.Category.Name ?? string.Empty,
                CreatedAt = x.CreatedAt,
                UserId = x.UserId,
                PosterName = x.User != null
                    ? (x.User.FullName ?? x.User.LoginName ?? $"User {x.User.Id}")
                    : "Không rõ",
                ImageUrls = x.Images
                    .OrderBy(img => img.Id)
                    .Select(img => img.ImageUrl ?? string.Empty)
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .ToList()
            })
            .ToListAsync();
    }
    public Task<Listing?> GetListingByIdWithUserAsync(int listingId)
        => _context.Listings
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == listingId);

    public Task UpdateAsync(Listing listing)
    {
        _context.Listings.Update(listing);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Listing listing)
    {
        _context.Listings.Remove(listing);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
