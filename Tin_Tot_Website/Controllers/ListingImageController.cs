using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TinTot.Application.DTOs.Users;
using TinTot.Application.Interfaces.Images;

namespace Tin_Tot_Website.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/listing-images")]
    public class ListingImageController : ControllerBase
    {
        private const long MaxImageSizeInBytes = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif"
        };

        private readonly IListingImageService _listingImageService;

        public ListingImageController(IListingImageService listingImageService)
        {
            _listingImageService = listingImageService;
        }

        [HttpPost]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> Create([FromForm] int listingId, IFormFile image)
        {
            try
            {
                var upload = await ToImageUploadAsync(image);
                if (upload is null)
                    return BadRequest(new { message = "Image là bắt buộc." });

                var result = await _listingImageService.CreateAsync(listingId, upload);
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

        [HttpPut("{id:int}")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> Update(int id, [FromForm] int listingId, IFormFile image)
        {
            try
            {
                var upload = await ToImageUploadAsync(image);
                if (upload is null)
                    return BadRequest(new { message = "Image là bắt buộc." });

                var result = await _listingImageService.UpdateAsync(id, listingId, upload);
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

        [HttpDelete("by-listing/{listingId:int}")]
        public async Task<IActionResult> DeleteByListingId(int listingId)
        {
            try
            {
                await _listingImageService.DeleteByListingIdAsync(listingId);
                return Ok(new { message = "Xóa toàn bộ ảnh của bài đăng thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
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
            if (image.Length > MaxImageSizeInBytes)
                throw new InvalidOperationException("Ảnh vượt quá 5MB.");

            if (!AllowedImageContentTypes.Contains(image.ContentType))
                throw new InvalidOperationException("Định dạng ảnh không hợp lệ.");
        }
    }
}
