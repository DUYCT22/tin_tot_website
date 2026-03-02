using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TinTot.Application.DTOs;
using TinTot.Application.DTOs.Users;
using TinTot.Application.Interfaces.Categories;

namespace Tin_Tot_Website.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
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

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [Authorize(Policy = "CategoryManagePolicy")]
        [HttpPost]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> Create([FromForm] string name, [FromForm] int? parentId, IFormFile? image)
        {
            try
            {
                var actorUserId = GetActorUserId(); if (actorUserId is null) return Unauthorized();
                var upload = await ToImageUploadAsync(image);
                var result = await _categoryService.CreateAsync(new CategoryUpsertDto { Name = name, ParentId = parentId, ActorUserId = actorUserId }, upload);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "CategoryManagePolicy")]
        [HttpPut("{id:int}")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> Update(int id, [FromForm] string name, [FromForm] int? parentId, IFormFile? image)
        {
            try
            {
                var actorUserId = GetActorUserId(); if (actorUserId is null) return Unauthorized();
                var upload = await ToImageUploadAsync(image);
                var result = await _categoryService.UpdateAsync(id, new CategoryUpsertDto { Name = name, ParentId = parentId, ActorUserId = actorUserId }, upload);
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

        [Authorize(Policy = "AdminOnlyPolicy")]
        [HttpDelete("{id:int}")]
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
                throw new InvalidOperationException("Ảnh banner vượt quá 5MB.");

            if (!AllowedImageContentTypes.Contains(image.ContentType))
                throw new InvalidOperationException("Định dạng ảnh không hợp lệ.");
        }
    }
}
