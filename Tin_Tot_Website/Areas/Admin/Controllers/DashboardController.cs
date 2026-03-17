using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tin_Tot_Website.Areas.Admin.Models;
using TinTot.Application.Interfaces.Admin;
using TinTot.Infrastructure.Data;

namespace Tin_Tot_Website.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminPortalAccessPolicy")]
[Route("admin")]
public class DashboardController : Controller
{
    private readonly IAdminDashboardService _adminDashboardService;
    private readonly AppDbContext _dbContext;
    private readonly Tin_Tot_Website.Services.IEntityKeyService _entityKeyService;
    private const int AdminRole = 1;
    private const int ListingManagerRole = 2;
    private const int UserManagerRole = 3;
    public DashboardController(IAdminDashboardService adminDashboardService, AppDbContext dbContext, Tin_Tot_Website.Services.IEntityKeyService entityKeyService)
    {
        _adminDashboardService = adminDashboardService;
        _dbContext = dbContext;
        _entityKeyService = entityKeyService;
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
            Dashboard = data,
            QuickChatTargets = await BuildQuickChatTargetsAsync(role)
        };

        return View(model);
    }
    private async Task<List<AdminQuickChatTargetViewModel>> BuildQuickChatTargetsAsync(int currentRole)
    {
        var activeUsers = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Status)
            .Select(x => new { x.Id, x.Role, Name = x.FullName ?? x.LoginName ?? $"User {x.Id}" })
            .OrderBy(x => x.Id)
            .ToListAsync();

        var currentUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedId) ? parsedId : 0;

        int[] targetRoles = currentRole switch
        {
            AdminRole => [ListingManagerRole, UserManagerRole],
            ListingManagerRole => [AdminRole, UserManagerRole],
            UserManagerRole => [AdminRole, ListingManagerRole],
            _ => [AdminRole, ListingManagerRole]
        };

        var targets = new List<AdminQuickChatTargetViewModel>();

        foreach (var role in targetRoles)
        {
            var target = activeUsers.FirstOrDefault(x => x.Role == role && x.Id != currentUserId);
            if (target is null) continue;

            targets.Add(new AdminQuickChatTargetViewModel
            {
                Title = role == AdminRole ? "Tin nhắn nhanh tới Admin" : role == ListingManagerRole ? "Tin nhắn nhanh tới Quản lý tin" : "Tin nhắn nhanh tới Quản lý user",
                ReceiverKey = _entityKeyService.ProtectId("seller", target.Id),
                ReceiverName = target.Name
            });
        }

        return targets;
    }
}
