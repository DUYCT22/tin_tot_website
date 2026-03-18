using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tin_Tot_Website.Services;
using TinTot.Application.Interfaces.Admin;

namespace Tin_Tot_Website.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "ListingManagePolicy")]
[Route("admin/duyet-tin")]
public class ListingApprovalController : Controller
{
    private readonly IAdminListingModerationService _service;
    private readonly IEntityKeyService _entityKeyService;

    public ListingApprovalController(IAdminListingModerationService service, IEntityKeyService entityKeyService)
    {
        _service = service;
        _entityKeyService = entityKeyService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var model = await _service.GetPendingListingsAsync(page: 1, pageSize: 4);
        PopulateMessageKeys(model.Listings);
        return View(model);
    }

    [HttpGet("tai-them")]
    public async Task<IActionResult> LoadMore([FromQuery] int page = 1, [FromQuery] int pageSize = 4)
    {
        var model = await _service.GetPendingListingsAsync(page, pageSize);
        PopulateMessageKeys(model.Listings);
        return PartialView("_PendingListingRows", model.Listings);
    }
    [HttpGet("/admin/tin-dang-hien-thi")]
    public async Task<IActionResult> Visible()
    {
        var model = await _service.GetApprovedListingsAsync(page: 1, pageSize: 4);
        PopulateMessageKeys(model.Listings);
        return View(model);
    }

    [HttpGet("/admin/tin-dang-hien-thi/tai-them")]
    public async Task<IActionResult> LoadMoreVisible([FromQuery] int page = 1, [FromQuery] int pageSize = 4)
    {
        var model = await _service.GetApprovedListingsAsync(page, pageSize);
        PopulateMessageKeys(model.Listings);
        return PartialView("_VisibleListingRows", model.Listings);
    }
    [HttpGet("/admin/tin-dang-hien-thi/danh-sach-xuat-excel")]
    public async Task<IActionResult> GetVisibleListingsForExcelExport()
    {
        var listings = await _service.GetApprovedListingsForExportSelectionAsync();
        var payload = listings.Select(x => new
        {
            id = x.ListingId,
            title = x.Title,
            posterName = x.PosterName,
            createdAt = x.CreatedAt?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? string.Empty
        });

        return Ok(new { success = true, data = payload });
    }

    [HttpPost("/admin/tin-dang-hien-thi/xuat-excel")]
    public async Task<IActionResult> ExportVisibleListingsExcel([FromBody] ExportVisibleListingsRequest request)
    {
        try
        {
            var result = await _service.ExportApprovedListingsExcelAsync(request.ExportAll, request.ListingIds);
            return File(result.Content, result.ContentType, result.FileName);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
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
    [HttpDelete("dang-hien-thi/{id:int}")]
    public async Task<IActionResult> DeleteVisible(int id)
    {
        try
        {
            await _service.DeleteApprovedListingAsync(id);
            return Ok(new { success = true, message = "Đã xóa tin đang hiển thị và gửi thông báo cho người đăng." });
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

    private void PopulateMessageKeys(IReadOnlyList<TinTot.Application.DTOs.Admin.AdminPendingListingItemDto> listings)
    {
        foreach (var item in listings)
        {
            item.ListingKey = _entityKeyService.ProtectId("listing", item.ListingId);
            item.PosterKey = item.UserId.HasValue
                ? _entityKeyService.ProtectId("seller", item.UserId.Value)
                : string.Empty;
        }
    }
    public class ExportVisibleListingsRequest
    {
        public bool ExportAll { get; set; }
        public List<int> ListingIds { get; set; } = new();
    }
}
