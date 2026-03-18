using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TinTot.Application.Interfaces.Admin;

namespace Tin_Tot_Website.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "ListingManagePolicy")]
[Route("admin/duyet-tin")]
public class ListingApprovalController : Controller
{
    private readonly IAdminListingModerationService _service;

    public ListingApprovalController(IAdminListingModerationService service)
    {
        _service = service;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var model = await _service.GetPendingListingsAsync(page: 1, pageSize: 4);
        return View(model);
    }

    [HttpGet("tai-them")]
    public async Task<IActionResult> LoadMore([FromQuery] int page = 1, [FromQuery] int pageSize = 4)
    {
        var model = await _service.GetPendingListingsAsync(page, pageSize);
        return PartialView("_PendingListingRows", model.Listings);
    }

    [HttpPost("{id:int}/duyet")]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            await _service.ApproveListingAsync(id);
            return Ok(new { success = true, message = "Duyệt bài đăng thành công." });
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Reject(int id)
    {
        try
        {
            await _service.RejectListingAsync(id);
            return Ok(new { success = true, message = "Đã xóa bài đăng vi phạm và gửi thông báo cho người đăng." });
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
}
