using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tin_Tot_Website.Services;
using TinTot.Application.Interfaces.Listings;

namespace Tin_Tot_Website.Controllers
{
    [ApiController]
    [Route("api/interactions")]
    public class InteractionController : ControllerBase
    {
        private readonly IInteractionService _interactionService;
        private readonly IEntityKeyService _entityKeyService;

        public InteractionController(IInteractionService interactionService, IEntityKeyService entityKeyService)
        {
            _interactionService = interactionService;
            _entityKeyService = entityKeyService;
        }

        [Authorize]
        [HttpGet("state")]
        public async Task<IActionResult> GetState([FromQuery] string? listingKey, [FromQuery] string? sellerKey)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Phiên đăng nhập không hợp lệ." });
            }

            var isFavorited = false;
            var isFollowing = false;

            if (!string.IsNullOrWhiteSpace(listingKey))
            {
                var listingId = _entityKeyService.UnprotectId("listing", listingKey);
                if (listingId.HasValue)
                {
                    isFavorited = await _interactionService.IsFavoritedAsync(userId.Value, listingId.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(sellerKey))
            {
                var sellerId = _entityKeyService.UnprotectId("seller", sellerKey);
                if (sellerId.HasValue)
                {
                    isFollowing = await _interactionService.IsFollowingSellerAsync(userId.Value, sellerId.Value);
                }
            }

            return Ok(new { success = true, isFavorited, isFollowing });
        }

        [Authorize]
        [HttpPost("favorites/{listingKey}")]
        public async Task<IActionResult> Favorite(string listingKey)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Phiên đăng nhập không hợp lệ." });
            }

            var listingId = _entityKeyService.UnprotectId("listing", listingKey);
            if (!listingId.HasValue)
            {
                return BadRequest(new { success = false, message = "Mã bài đăng không hợp lệ." });
            }

            try
            {
                var message = await _interactionService.AddFavoriteAsync(userId.Value, listingId.Value);
                return Ok(new { success = true, message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("favorites/{listingKey}")]
        public async Task<IActionResult> Unfavorite(string listingKey)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Phiên đăng nhập không hợp lệ." });
            }

            var listingId = _entityKeyService.UnprotectId("listing", listingKey);
            if (!listingId.HasValue)
            {
                return BadRequest(new { success = false, message = "Mã bài đăng không hợp lệ." });
            }

            try
            {
                var message = await _interactionService.RemoveFavoriteAsync(userId.Value, listingId.Value);
                return Ok(new { success = true, message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("follows/{sellerKey}")]
        public async Task<IActionResult> Follow(string sellerKey)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Phiên đăng nhập không hợp lệ." });
            }

            var sellerId = _entityKeyService.UnprotectId("seller", sellerKey);
            if (!sellerId.HasValue)
            {
                return BadRequest(new { success = false, message = "Mã người bán không hợp lệ." });
            }

            try
            {
                var message = await _interactionService.FollowSellerAsync(userId.Value, sellerId.Value);
                return Ok(new { success = true, message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("follows/{sellerKey}")]
        public async Task<IActionResult> Unfollow(string sellerKey)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Phiên đăng nhập không hợp lệ." });
            }

            var sellerId = _entityKeyService.UnprotectId("seller", sellerKey);
            if (!sellerId.HasValue)
            {
                return BadRequest(new { success = false, message = "Mã người bán không hợp lệ." });
            }

            try
            {
                var message = await _interactionService.UnfollowSellerAsync(userId.Value, sellerId.Value);
                return Ok(new { success = true, message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        private int? GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}
