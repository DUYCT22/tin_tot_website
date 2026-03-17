using TinTot.Application.DTOs.Admin;

namespace Tin_Tot_Website.Areas.Admin.Models;

public class AdminDashboardPageViewModel
{
    public string UserName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int Role { get; set; }
    public AdminDashboardDto Dashboard { get; set; } = new();
}
