using ClosedXML.Excel;
using TinTot.Application.DTOs.Admin;
using TinTot.Application.Interfaces.Admin;

namespace TinTot.Infrastructure.Services;

public class VisibleListingsExcelExporter : IVisibleListingsExcelExporter
{
    public byte[] Export(IReadOnlyList<AdminPendingListingItemDto> listings)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("TinDangHienThi");

        var headers = new[]
        {
            "STT", "ID", "Tiêu đề", "Mô tả", "Vị trí", "Danh mục", "Tạo lúc", "Người đăng", "Trạng thái", "Hình ảnh"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        var headerRange = worksheet.Range(1, 1, 1, headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        for (var index = 0; index < listings.Count; index++)
        {
            var row = index + 2;
            var item = listings[index];

            worksheet.Cell(row, 1).Value = index + 1;
            worksheet.Cell(row, 2).Value = item.ListingId;
            worksheet.Cell(row, 3).Value = item.Title;
            worksheet.Cell(row, 4).Value = item.Description;
            worksheet.Cell(row, 5).Value = item.Location;
            worksheet.Cell(row, 6).Value = item.CategoryName;
            worksheet.Cell(row, 7).Value = item.CreatedAt?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? string.Empty;
            worksheet.Cell(row, 8).Value = item.PosterName;
            worksheet.Cell(row, 9).Value = "Đang hiển thị";
            worksheet.Cell(row, 10).Value = string.Join(Environment.NewLine, item.ImageUrls);
        }

        var usedRange = worksheet.RangeUsed();
        if (usedRange != null)
        {
            usedRange.Style.Font.FontName = "Times New Roman";
            usedRange.Style.Font.FontSize = 14;
            usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            usedRange.Style.Alignment.WrapText = true;
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }

        worksheet.Columns().AdjustToContents(12, 70);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
