using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tin_Tot_Website.Models.Notifications;
using TinTot.Application.Interfaces.Notifications;

namespace Tin_Tot_Website.Controllers
{
    [Authorize]
    [Route("api/notifications")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            var notifications = await _notificationService.GetRecentAsync(userId.Value, 20);
            var vm = notifications.Select(x => new NotificationItemViewModel
            {
                Id = x.Id,
                Message = x.Message,
                CreatedAt = x.CreatedAt,
                IsRead = x.IsRead
            }).ToList();

            return PartialView("~/Views/Shared/_NotificationList.cshtml", vm);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            var count = await _notificationService.GetUnreadCountAsync(userId.Value);
            return Ok(new { unreadCount = count });
        }

        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            await _notificationService.MarkAllAsReadAsync(userId.Value);
            return Ok(new { success = true });
        }

        private int? GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}
