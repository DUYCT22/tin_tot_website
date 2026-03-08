using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tin_Tot_Website.Models;
using Tin_Tot_Website.Services;
using TinTot.Application.Common;
using TinTot.Application.Interfaces.Users;
using TinTot.Infrastructure.Data;

namespace Tin_Tot_Website.Controllers
{
    [Authorize]
    public class MemberListingController : Controller
    {
        private const long MaxAvatarSizeInBytes = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedAvatarContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp", "image/gif"
        };

        private readonly AppDbContext _dbContext;
        private readonly IEntityKeyService _entityKeyService;
        private readonly IAvatarStorageService _avatarStorageService;

        public MemberListingController(
            AppDbContext dbContext,
            IEntityKeyService entityKeyService,
            IAvatarStorageService avatarStorageService)
        {
            _dbContext = dbContext;
            _entityKeyService = entityKeyService;
            _avatarStorageService = avatarStorageService;
        }

        [HttpGet("dang-tin")]
        public async Task<IActionResult> Post()
        {
            var parentCategories = await _dbContext.Categories
                .AsNoTracking()
                .Where(c => c.ParentId == null)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(new ListingPostPageViewModel
            {
                ParentCategories = parentCategories
            });
        }

        [HttpGet("api/listing-form/sub-categories")]
        public async Task<IActionResult> GetSubCategories([FromQuery] int parentId)
        {
            var subCategories = await _dbContext.Categories
                .AsNoTracking()
                .Where(c => c.ParentId == parentId)
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return Ok(subCategories);
        }

        [HttpGet("tin-da-luu")]
        public async Task<IActionResult> Saved()
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Challenge();
            }

            var favorites = await _dbContext.Favorites
                .AsNoTracking()
                .Where(f => f.UserId == currentUserId.Value)
                .Include(f => f.Listing)
                    .ThenInclude(l => l.Images)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new SavedListingViewModel
                {
                    FavoriteId = f.Id,
                    ListingKey = _entityKeyService.ProtectId("listing", f.ListingId),
                    Title = f.Listing.Title ?? "Không có tiêu đề",
                    Price = f.Listing.Price ?? 0,
                    Location = f.Listing.Location ?? "Chưa có địa chỉ",
                    ImageUrl = f.Listing.Images
                        .OrderBy(i => i.Id)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault() ?? string.Empty,
                    SavedAt = f.CreatedAt,
                    DetailUrl = Url.Action("Detail", "PublicListing", new
                    {
                        slug = SlugHelper.ToSlug(f.Listing.Title ?? "tin-dang"),
                        key = _entityKeyService.ProtectId("listing", f.ListingId)
                    }) ?? "#"
                })
                .ToListAsync();

            return View(favorites);
        }

        [HttpGet("trang-ca-nhan")]
        public async Task<IActionResult> Profile()
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Challenge();
            }

            var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == currentUserId.Value);
            if (user is null)
            {
                return NotFound();
            }

            var listings = await _dbContext.Listings
                .AsNoTracking()
                .Where(x => x.UserId == currentUserId.Value)
                .Include(x => x.Images)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ProfileListingItemViewModel
                {
                    Id = x.Id,
                    CategoryId = x.CategoryId,
                    Title = x.Title ?? "Không có tiêu đề",
                    Price = x.Price ?? 0,
                    Status = x.Status,
                    ImageUrl = x.Images.OrderBy(i => i.Id).Select(i => i.ImageUrl).FirstOrDefault() ?? string.Empty
                })
                .ToListAsync();

            var followers = await _dbContext.Follows
                .AsNoTracking()
                .Where(x => x.SellerId == currentUserId.Value)
                .Include(x => x.Follower)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ProfileFollowerItemViewModel
                {
                    FullName = x.Follower.FullName ?? "Người dùng",
                    Avatar = x.Follower.Avatar ?? string.Empty,
                    FollowedAt = x.CreatedAt
                })
                .ToListAsync();

            var ratings = await _dbContext.Ratings
                .AsNoTracking()
                .Where(x => x.UserId == currentUserId.Value)
                .Include(x => x.Reviewer)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ProfileRatingItemViewModel
                {
                    ReviewerName = x.Reviewer.FullName ?? "Ẩn danh",
                    Score = x.Score ?? 0,
                    Comment = x.Comment ?? string.Empty,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            var categories = await _dbContext.Categories
                .AsNoTracking()
                .OrderBy(x => x.ParentId)
                .ThenBy(x => x.Name)
                .Select(x => new ProfileCategoryOptionViewModel
                {
                    Id = x.Id,
                    Name = x.Name ?? "Danh mục",
                    ParentId = x.ParentId
                })
                .ToListAsync();

            var vm = new ProfilePageViewModel
            {
                User = new UserProfileSummaryViewModel
                {
                    FullName = user.FullName ?? "Người dùng",
                    Email = user.Email ?? string.Empty,
                    Phone = user.Phone ?? string.Empty,
                    Avatar = user.Avatar ?? string.Empty,
                    CreatedAt = user.CreatedAt,
                    FollowersCount = followers.Count,
                    RatingsCount = ratings.Count
                },
                ListingShowing = listings.Where(x => x.Status == 1).ToList(),
                ListingPendingApproval = listings.Where(x => x.Status == 0).ToList(),
                ListingSold = listings.Where(x => x.Status == 2).ToList(),
                Followers = followers,
                Ratings = ratings,
                Categories = categories
            };

            return View(vm);
        }

        [HttpPost("api/member/profile/update")]
        public async Task<IActionResult> UpdateProfile([FromForm] string fullName, [FromForm] string phone)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Phiên đăng nhập không hợp lệ." });
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == currentUserId.Value);
            if (user is null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy người dùng." });
            }

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone))
            {
                return BadRequest(new { success = false, message = "Vui lòng nhập đầy đủ họ tên và số điện thoại." });
            }

            var normalizedPhone = phone.Trim();
            var phoneExists = await _dbContext.Users.AnyAsync(x => x.Id != user.Id && x.Phone == normalizedPhone);
            if (phoneExists)
            {
                return Conflict(new { success = false, message = "Số điện thoại đã được sử dụng." });
            }

            user.FullName = fullName.Trim();
            user.Phone = normalizedPhone;
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Cập nhật thông tin thành công.",
                fullName = user.FullName,
                phone = user.Phone
            });
        }

        [HttpPost("api/member/profile/avatar")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UpdateAvatar([FromForm] IFormFile avatar)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Phiên đăng nhập không hợp lệ." });
            }

            if (avatar is null || avatar.Length == 0)
            {
                return BadRequest(new { success = false, message = "Vui lòng chọn ảnh." });
            }

            if (avatar.Length > MaxAvatarSizeInBytes)
            {
                return BadRequest(new { success = false, message = "Ảnh vượt quá 5MB." });
            }

            if (!AllowedAvatarContentTypes.Contains(avatar.ContentType))
            {
                return BadRequest(new { success = false, message = "Định dạng ảnh không hợp lệ." });
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == currentUserId.Value);
            if (user is null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy người dùng." });
            }

            await using var stream = avatar.OpenReadStream();
            var avatarUrl = await _avatarStorageService.UploadImageAsync(stream, $"User/{user.Id}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
            user.Avatar = avatarUrl;
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "Cập nhật ảnh đại diện thành công.", avatarUrl });
        }

        [HttpPost("api/member/listings/{id:int}/mark-sold")]
        public async Task<IActionResult> MarkAsSold(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Phiên đăng nhập không hợp lệ." });
            }

            var listing = await _dbContext.Listings.FirstOrDefaultAsync(x => x.Id == id && x.UserId == currentUserId.Value);
            if (listing is null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy bài đăng." });
            }

            listing.Status = 2;
            listing.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã chuyển trạng thái bài đăng sang đã bán." });
        }

        [HttpPut("api/member/listings/{id:int}/quick-update")]
        public async Task<IActionResult> QuickUpdateListing(int id, [FromBody] QuickUpdateListingRequest request)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Phiên đăng nhập không hợp lệ." });
            }

            var listing = await _dbContext.Listings.FirstOrDefaultAsync(x => x.Id == id && x.UserId == currentUserId.Value);
            if (listing is null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy bài đăng." });
            }

            var categoryExists = await _dbContext.Categories.AnyAsync(x => x.Id == request.CategoryId);
            if (!categoryExists)
            {
                return BadRequest(new { success = false, message = "Danh mục không hợp lệ." });
            }

            if (request.Price < 0)
            {
                return BadRequest(new { success = false, message = "Giá không hợp lệ." });
            }

            listing.CategoryId = request.CategoryId;
            listing.Price = request.Price;
            listing.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã cập nhật danh mục và giá." });
        }

        [HttpGet("api/member/categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _dbContext.Categories
                .AsNoTracking()
                .OrderBy(x => x.ParentId)
                .ThenBy(x => x.Name)
                .Select(x => new { x.Id, x.Name, x.ParentId })
                .ToListAsync();

            return Ok(categories);
        }

        private int? GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : null;
        }

        public class QuickUpdateListingRequest
        {
            public int CategoryId { get; set; }
            public decimal Price { get; set; }
        }
    }
}
