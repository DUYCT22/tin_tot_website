using Microsoft.AspNetCore.Mvc;
using Tin_Tot_Website.Data;
using Tin_Tot_Website.Models;

namespace Tin_Tot_Website.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        public UserController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost("seed")]
        public async Task<IActionResult> Seed()
        {
            var user = new User
            {
                FullName = "Test User",
                Email = "test@mail.com",
                Role = 0,
                Online = false,
                Status = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var category = new Category
            {
                Name = "Test Category"
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var listing = new Listing
            {
                Title = "Test Listing",
                Description = "Test Description",
                UserId = user.Id,
                CategoryId = category.Id,
                Status = 1,
                CreatedAt = DateTime.UtcNow
            };

            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();

            return Ok("Seed completed");
        }
        [HttpDelete("del")]
        public async Task<IActionResult> Del()
        {
            var user = await _context.Users.FindAsync(1);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok("Post user success");
        }
    }
}
