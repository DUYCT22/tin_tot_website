using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tin_Tot_Website.Areas.Admin.Models;
using TinTot.Application.DTOs;
using TinTot.Application.DTOs.Users;
using TinTot.Application.Interfaces.Categories;
using TinTot.Infrastructure.Data;

namespace Tin_Tot_Website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnlyPolicy")]
    [Route("admin/danh-muc")]
    public class CategoryController : Controller
    {
        private const long MaxAvatarSizeInBytes = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif"
        };
        private readonly ICategoryService _categoryService;
        private readonly AppDbContext _dbContext;
        public CategoryController(ICategoryService categoryService, AppDbContext dbContext)
        {
            _categoryService = categoryService;
            _dbContext = dbContext;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var categories = await _dbContext.Categories
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => new CategoryManagementItemViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    ParentId = x.ParentId,
                    ParentName = x.Parent != null ? x.Parent.Name : null,
                    Image = x.Image,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy
                })
                .ToListAsync();

            var parentOptions = categories
                .Select(x => new CategoryParentOptionViewModel
                {
                    Id = x.Id,
                    Name = x.Name ?? $"Danh mục #{x.Id}"
                })
                .ToList();

            return View(new CategoryManagementPageViewModel
            {
                Categories = categories,
                ParentOptions = parentOptions
            });
        }

        [HttpPost("/api/categories")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> Create([FromForm] string name, [FromForm] int? parentId, IFormFile? image)
        {
            try
            {
                var actorUserId = GetActorUserId();
                if (actorUserId is null) return Unauthorized();
                var upload = await ToImageUploadAsync(image);
                var result = await _categoryService.CreateAsync(new CategoryUpsertDto
                {
                    Name = name,
                    ParentId = parentId,
                    ActorUserId = actorUserId
                }, upload);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("/api/categories/{id:int}")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> Update(int id, [FromForm] string name, [FromForm] int? parentId, IFormFile? image)
        {
            try
            {
                var actorUserId = GetActorUserId();
                if (actorUserId is null) return Unauthorized();
                var upload = await ToImageUploadAsync(image);
                var result = await _categoryService.UpdateAsync(id, new CategoryUpsertDto
                {
                    Name = name,
                    ParentId = parentId,
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

        [HttpDelete("/api/categories/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _categoryService.DeleteAsync(id);
                return Ok(new { message = "Xóa category thành công." });
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
                throw new InvalidOperationException("Ảnh danh mục vượt quá 5MB.");

            if (!AllowedImageContentTypes.Contains(image.ContentType))
                throw new InvalidOperationException("Định dạng ảnh không hợp lệ.");
        }
    }
}
