using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics;
using System.Text.Json;
using Tin_Tot_Website.Data;
using Tin_Tot_Website.Models;

namespace Tin_Tot_Website.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            var cacheKey = "home_listings";

            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var cachedListings = JsonSerializer.Deserialize<List<Listing>>(cachedData);
                Console.WriteLine("CACHE HIT");
                return View(cachedListings);
            }

            Console.WriteLine("CACHE MISS - Load from DB");

            var listings = await _context.Listings
                .Include(l => l.Images)
                .ToListAsync();

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(listings),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                });

            return View(listings);
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
    }
}
