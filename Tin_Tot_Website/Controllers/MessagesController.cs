using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using Tin_Tot_Website.Models.Messages;
using Tin_Tot_Website.Services;
using TinTot.Application.DTOs.Messages;
using TinTot.Application.Interfaces.Messages;

namespace Tin_Tot_Website.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IEntityKeyService _entityKeyService;

        public MessagesController(IMessageService messageService, IEntityKeyService entityKeyService)
        {
            _messageService = messageService;
            _entityKeyService = entityKeyService;
        }

        [HttpGet("Tin-nhan")]
        public IActionResult Index([FromQuery] string? receiverKey, [FromQuery] string? listingKey)
            => View(new MessagesPageViewModel { ReceiverKey = receiverKey, ListingKey = listingKey });

        [HttpGet("api/messages/conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var data = await _messageService.GetConversationsAsync(userId.Value);
            var payload = data.Select(x => new
            {
                receiverId = x.UserId,
                receiverKey = _entityKeyService.ProtectId("seller", x.UserId),
                displayName = x.DisplayName,
                avatar = x.Avatar,
                lastMessage = x.LastMessage,
                lastSentAt = x.LastSentAt
            });

            return Ok(new { success = true, data = payload });
        }

        [HttpGet("api/messages/history/{receiverKey}")]
        public async Task<IActionResult> GetHistory(string receiverKey)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var receiverId = _entityKeyService.UnprotectId("seller", receiverKey);
            if (!receiverId.HasValue) return BadRequest(new { success = false, message = "Người nhận không hợp lệ." });

            var data = await _messageService.GetHistoryAsync(userId.Value, receiverId.Value, 100);
            var payload = data.Select(x => new
            {
                x.Id,
                x.SenderId,
                x.ReceiverId,
                x.Content,
                x.SentAt,
                x.SenderName,
                x.ReceiverName,
                listingKey = x.ListingId.HasValue ? _entityKeyService.ProtectId("listing", x.ListingId.Value) : null,
                x.ListingTitle,
                x.ListingImageUrl
            });

            return Ok(new { success = true, data = payload });
        }
        [EnableRateLimiting("MessageSendPolicy")]
        [HttpPost("api/messages/send")]
        public async Task<IActionResult> Send([FromBody] SendMessageRequestModel request)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var receiverId = _entityKeyService.UnprotectId("seller", request.ReceiverKey);
            if (!receiverId.HasValue) return BadRequest(new { success = false, message = "Người nhận không hợp lệ." });

            int? listingId = null;
            if (!string.IsNullOrWhiteSpace(request.ListingKey))
            {
                listingId = _entityKeyService.UnprotectId("listing", request.ListingKey);
                if (!listingId.HasValue)
                {
                    return BadRequest(new { success = false, message = "Tin đăng đính kèm không hợp lệ." });
                }
            }

            try
            {
                var sent = await _messageService.SendAsync(new SendMessageRequestDto
                {
                    SenderId = userId.Value,
                    ReceiverId = receiverId.Value,
                    ListingId = listingId,
                    Content = request.Content
                });

                return Ok(new { success = true, data = sent });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
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
