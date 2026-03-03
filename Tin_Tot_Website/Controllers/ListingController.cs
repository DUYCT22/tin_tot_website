using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TinTot.Application.DTOs.Listing;
using TinTot.Application.DTOs.Users;
using TinTot.Application.Interfaces.Listings;

namespace Tin_Tot_Website.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/listings")]
    public class ListingController : ControllerBase
    {
        private const long MaxImageSizeInBytes = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif"
        };
        private readonly IListingService _listingService;

        public ListingController(IListingService listingService)
        {
            _listingService = listingService;
        }

        [HttpPost]
        [RequestSizeLimit(30_000_000)]
        public async Task<IActionResult> Create(
            [FromForm] int categoryId,
            [FromForm] string title,
            [FromForm] string description,
            [FromForm] decimal price,
            [FromForm] string location,
            [FromForm] int status,
            [FromForm] List<IFormFile> images)
        {
            try
            {
                var uploads = await ToImageUploadsAsync(images);
                var actorUserId = GetActorUserId(); if (actorUserId is null) return Unauthorized();
                var dto = new ListingCreateDto
                {
                    ActorUserId = actorUserId,
                    CategoryId = categoryId,
                    Title = title,
                    Description = description,
                    Price = price,
                    Location = location,
                    Status = status
                };

                var result = await _listingService.CreateAsync(dto, uploads);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ListingUpdateDto dto)
        {
            try
            {
                var result = await _listingService.UpdateAsync(id, dto);
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

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _listingService.DeleteAsync(id);
                return Ok(new { message = "Xóa bài đăng thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        private static async Task<IReadOnlyCollection<AvatarUploadDto>> ToImageUploadsAsync(IReadOnlyCollection<IFormFile>? images)
        {
            if (images is null || images.Count == 0)
                return Array.Empty<AvatarUploadDto>();

            var uploads = new List<AvatarUploadDto>();
            foreach (var image in images)
            {
                if (image.Length == 0)
                    continue;

                ValidateImage(image);

                await using var buffer = new MemoryStream();
                await image.CopyToAsync(buffer);

                uploads.Add(new AvatarUploadDto
                {
                    FileName = image.FileName,
                    Content = new MemoryStream(buffer.ToArray())
                });
            }

            return uploads;
        }
        private static void ValidateImage(IFormFile image)
        {
            if (image.Length > MaxImageSizeInBytes)
                throw new InvalidOperationException("Ảnh vượt quá 5MB.");

            if (!AllowedImageContentTypes.Contains(image.ContentType))
                throw new InvalidOperationException("Định dạng ảnh không hợp lệ.");
        }
        private int? GetActorUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}
