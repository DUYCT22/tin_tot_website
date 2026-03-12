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

        [HttpGet("Tat-ca-bai-dang")]
        public async Task<IActionResult> AllListings([FromQuery] int? categoryId, [FromQuery] string? keyword, [FromQuery] string? sort)
        {
            var currentUserId = GetCurrentUserId();
            var model = await _homeQueryService.GetAllListingsPageDataAsync(currentUserId, categoryId, keyword, sort, page: 1, pageSize: 12);
            return View(model);
        }

        [HttpGet("Tat-ca-bai-dang/tai-them")]
        public async Task<IActionResult> AllListingsLoadMore([FromQuery] int page = 1, [FromQuery] int pageSize = 12, [FromQuery] int? categoryId = null, [FromQuery] string? keyword = null, [FromQuery] string? sort = null)
        {
            var currentUserId = GetCurrentUserId();
            var model = await _homeQueryService.GetAllListingsPageDataAsync(currentUserId, categoryId, sort, keyword, page, pageSize);

            return PartialView("~/Views/Shared/Components/_ListingCardItems.cshtml", new ListingCardSectionViewModel
            {
                Listings = model.Listings,
                UserRatingAvg = model.UserRatingAvg,
                EmptyMessage = string.Empty
            });
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
