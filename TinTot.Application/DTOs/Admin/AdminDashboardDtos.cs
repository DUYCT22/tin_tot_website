namespace TinTot.Application.DTOs.Admin;

public class AdminDashboardDto
{
    public int TotalListings { get; set; }
    public int TotalActiveListings { get; set; }
    public int TotalSoldListings { get; set; }
    public int TotalPendingListings { get; set; }
    public decimal ListingGrowthPercent { get; set; }
    public decimal NewUserGrowthPercent { get; set; }
    public int VisibleLeafCategories { get; set; }
    public int VisibleBanners { get; set; }
    public int ActiveMemberUsers { get; set; }
    public int InactiveMemberUsers { get; set; }
    public List<DashboardSliceDto> ListingDistribution { get; set; } = new();
    public List<DashboardBarItemDto> ListingsByCategory { get; set; } = new();
    public List<DashboardLineItemDto> MonthlyListingGrowth { get; set; } = new();
}

public class DashboardSliceDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percent { get; set; }
}

public class DashboardBarItemDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DashboardLineItemDto
{
    public string MonthLabel { get; set; } = string.Empty;
    public int Count { get; set; }
}
