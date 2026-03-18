using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tin_Tot_Website.Areas.Admin.Models;
using TinTot.Application.DTOs;
using TinTot.Application.DTOs.Users;
using TinTot.Application.Interfaces.Banners;
using TinTot.Infrastructure.Data;

namespace Tin_Tot_Website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnlyPolicy")]
    [Route("admin/banner")]
    public class BannerController : Controller
    {
        private const long MaxAvatarSizeInBytes = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif"
        };
        private readonly IBannerService _bannerService;
        private readonly AppDbContext _dbContext;
        public BannerController(IBannerService bannerService, AppDbContext dbContext)
        {
            _bannerService = bannerService;
            _dbContext = dbContext;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var banners = await _dbContext.Banners
                .AsNoTracking()
                .OrderBy(x => x.Orders)
                .ThenByDescending(x => x.Id)
                .Select(x => new BannerManagementItemViewModel
                {
                    Id = x.Id,
                    Link = x.Link,
                    Image = x.Image,
                    Status = x.Status,
                    Orders = x.Orders,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy
                })
                .ToListAsync();

            return View(new BannerManagementPageViewModel
            {
                Banners = banners
            });
        }

        [HttpPost("/api/banners")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> Create([FromForm] string link, [FromForm] bool status, [FromForm] int orders, IFormFile image)
        {
            try
            {
                var actorUserId = GetActorUserId();
                if (actorUserId is null) return Unauthorized();
                var upload = await ToImageUploadAsync(image);
                if (upload is null) return BadRequest(new { message = "Image là bắt buộc." });
                var result = await _bannerService.CreateAsync(new BannerUpsertDto
                {
                    Link = link,
                    Status = status,
                    Orders = orders,
                    ActorUserId = actorUserId
                }, upload);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("/api/banners/{id:int}")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> Update(int id, [FromForm] string link, [FromForm] bool status, [FromForm] int orders, IFormFile? image)
        {
            try
            {
                var upload = await ToImageUploadAsync(image);
                var actorUserId = GetActorUserId();
                var result = await _bannerService.UpdateAsync(id, new BannerUpsertDto
                {
                    Link = link,
                    Status = status,
                    Orders = orders,
                    ActorUserId = actorUserId
                }, upload);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("/api/banners/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _bannerService.DeleteAsync(id);
                return Ok(new { message = "Xóa banner thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        private int? GetActorUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : null;
        }
        private static async Task<AvatarUploadDto?> ToImageUploadAsync(IFormFile? image)
        {
            if (image is null || image.Length == 0)
                return null;

            ValidateImage(image);

            await using var buffer = new MemoryStream();
            await image.CopyToAsync(buffer);

            return new AvatarUploadDto
            {
                FileName = image.FileName,
                Content = new MemoryStream(buffer.ToArray())
            };
        }
        private static void ValidateImage(IFormFile image)
        {
            if (image.Length > MaxAvatarSizeInBytes)
                throw new InvalidOperationException("Ảnh banner vượt quá 5MB.");

            if (!AllowedImageContentTypes.Contains(image.ContentType))
                throw new InvalidOperationException("Định dạng ảnh không hợp lệ.");
        }
    }
}
