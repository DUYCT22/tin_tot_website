using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using Tin_Tot_Website.Models;
using TinTot.Application.Interfaces.Home;

namespace Tin_Tot_Website.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeQueryService _homeQueryService;

        public HomeController(IHomeQueryService homeQueryService)
        {
            _homeQueryService = homeQueryService;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();
            var model = await _homeQueryService.GetHomePageDataAsync(currentUserId, 6);
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}
