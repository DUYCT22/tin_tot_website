using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TinTot.Application.DTOs;
using TinTot.Application.DTOs.Users;
using TinTot.Application.Interfaces.Banners;

namespace Tin_Tot_Website.Areas.Admin.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/banners")]
    public class BannerController : ControllerBase
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

        public BannerController(IBannerService bannerService)
        {
            _bannerService = bannerService;
        }

        [Authorize(Policy = "BannerManagePolicy")]
        [HttpPost]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> Create([FromForm] string link, [FromForm] bool status, [FromForm] int orders, IFormFile image)
        {
            try
            {
                var actorUserId = GetActorUserId(); if (actorUserId is null) return Unauthorized();
                var upload = await ToImageUploadAsync(image);
                if (upload is null) return BadRequest(new { message = "Image là bắt buộc." });
                var result = await _bannerService.CreateAsync(new BannerUpsertDto { Link = link, Status = status, Orders = orders, ActorUserId = actorUserId }, upload);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "BannerManagePolicy")]
        [HttpPut("{id:int}")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> Update(int id, [FromForm] string link, [FromForm] bool status, [FromForm] int orders, IFormFile? image)
        {
            try
            {
                var upload = await ToImageUploadAsync(image);
                var actorUserId = GetActorUserId();
                var result = await _bannerService.UpdateAsync(id, new BannerUpsertDto { Link = link, Status = status, Orders = orders, ActorUserId = actorUserId }, upload);
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
