using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tin_Tot_Website.Areas.Admin.Models;
using TinTot.Application.Interfaces.Admin;

namespace Tin_Tot_Website.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminPortalAccessPolicy")]
[Route("admin")]
public class DashboardController : Controller
{
    private readonly IAdminDashboardService _adminDashboardService;

    public DashboardController(IAdminDashboardService adminDashboardService)
    {
        _adminDashboardService = adminDashboardService;
    }

    [HttpGet("")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> Index()
    {
        var data = await _adminDashboardService.GetDashboardDataAsync();
        var role = int.TryParse(User.FindFirstValue(ClaimTypes.Role), out var parsedRole) ? parsedRole : 0;

        var model = new AdminDashboardPageViewModel
        {
            UserName = User.FindFirstValue(ClaimTypes.Name) ?? "Admin",
            AvatarUrl = "https://ui-avatars.com/api/?background=ffb703&color=fff&name=" + Uri.EscapeDataString(User.FindFirstValue(ClaimTypes.Name) ?? "A"),
            Role = role,
            Dashboard = data
        };

        return View(model);
    }
}
