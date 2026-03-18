namespace Tin_Tot_Website.Areas.Admin.Models;

public class BannerManagementPageViewModel
{
    public IReadOnlyList<BannerManagementItemViewModel> Banners { get; set; } = [];
}

public class BannerManagementItemViewModel
{
    public int Id { get; set; }
    public string? Link { get; set; }
    public string? Image { get; set; }
    public bool Status { get; set; }
    public int Orders { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
}
