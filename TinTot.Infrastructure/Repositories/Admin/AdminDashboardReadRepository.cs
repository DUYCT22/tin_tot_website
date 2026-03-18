using Microsoft.EntityFrameworkCore;
using TinTot.Application.DTOs.Admin;
using TinTot.Application.Interfaces.Admin;
using TinTot.Infrastructure.Data;

namespace TinTot.Infrastructure.Repositories.Admin;

public class AdminDashboardRepository : IAdminDashboardRepository
{
    private readonly AppDbContext _context;

    public AdminDashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardDto> GetDashboardDataAsync()
    {
        var now = DateTime.UtcNow;
        var thisWeekStart = now.Date.AddDays(-(int)now.DayOfWeek);
        var lastWeekStart = thisWeekStart.AddDays(-7);

        var totalListings = await _context.Listings.AsNoTracking().CountAsync();
        var pending = await _context.Listings.AsNoTracking().CountAsync(x => x.Status == 0);
        var active = await _context.Listings.AsNoTracking().CountAsync(x => x.Status == 1);
        var sold = await _context.Listings.AsNoTracking().CountAsync(x => x.Status == 2);

        var thisWeekListings = await _context.Listings.AsNoTracking().CountAsync(x => x.CreatedAt >= thisWeekStart);
        var lastWeekListings = await _context.Listings.AsNoTracking().CountAsync(x => x.CreatedAt >= lastWeekStart && x.CreatedAt < thisWeekStart);
        var thisWeekMembers = await _context.Users.AsNoTracking().CountAsync(x => x.Role == 0 && x.CreatedAt >= thisWeekStart);
        var lastWeekMembers = await _context.Users.AsNoTracking().CountAsync(x => x.Role == 0 && x.CreatedAt >= lastWeekStart && x.CreatedAt < thisWeekStart);

        var leafCategoriesShowing = await _context.Categories
            .AsNoTracking()
            .Where(c => !_context.Categories.Any(child => child.ParentId == c.Id))
            .CountAsync(c => _context.Listings.Any(l => l.CategoryId == c.Id && l.Status == 1));

        var bannersShowing = await _context.Banners.AsNoTracking().CountAsync(x => x.Status);

        var memberActive = await _context.Users.AsNoTracking().CountAsync(x => x.Role == 0 && x.Status);
        var memberInactive = await _context.Users.AsNoTracking().CountAsync(x => x.Role == 0 && !x.Status);

        var monthly = new List<DashboardLineItemDto>();
        var monthCursor = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-5);
        for (var i = 0; i < 6; i++)
        {
            var monthStart = monthCursor.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);
            var count = await _context.Listings.AsNoTracking().CountAsync(x => x.CreatedAt >= monthStart && x.CreatedAt < monthEnd);
            monthly.Add(new DashboardLineItemDto
            {
                MonthLabel = monthStart.ToString("MM/yyyy"),
                Count = count
            });
        }

        var barData = await _context.Categories
            .AsNoTracking()
            .Where(c => c.ParentId != null)
            .Select(c => new DashboardBarItemDto
            {
                CategoryName = c.Name,
                Count = _context.Listings.Count(l => l.CategoryId == c.Id)
            })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync();

        var pieData = BuildPieData(pending, active, sold);

        return new AdminDashboardDto
        {
            TotalListings = totalListings,
            TotalActiveListings = active,
            TotalSoldListings = sold,
            TotalPendingListings = pending,
            ListingGrowthPercent = CalculateGrowth(lastWeekListings, thisWeekListings),
            NewUserGrowthPercent = CalculateGrowth(lastWeekMembers, thisWeekMembers),
            VisibleLeafCategories = leafCategoriesShowing,
            VisibleBanners = bannersShowing,
            ActiveMemberUsers = memberActive,
            InactiveMemberUsers = memberInactive,
            ListingDistribution = pieData,
            ListingsByCategory = barData,
            MonthlyListingGrowth = monthly
        };
    }

    private static decimal CalculateGrowth(int previous, int current)
    {
        if (previous == 0)
        {
            return current == 0 ? 0 : 100;
        }

        return Math.Round(((current - previous) / (decimal)previous) * 100, 2);
    }

    private static List<DashboardSliceDto> BuildPieData(int pending, int active, int sold)
    {
        var total = pending + active + sold;
        var safeTotal = total == 0 ? 1 : total;

        return new List<DashboardSliceDto>
        {
            new() { Label = "Chờ duyệt", Count = pending, Percent = Math.Round(pending * 100m / safeTotal, 2) },
            new() { Label = "Đang hiển thị", Count = active, Percent = Math.Round(active * 100m / safeTotal, 2) },
            new() { Label = "Đã bán", Count = sold, Percent = Math.Round(sold * 100m / safeTotal, 2) }
        };
    }
}
