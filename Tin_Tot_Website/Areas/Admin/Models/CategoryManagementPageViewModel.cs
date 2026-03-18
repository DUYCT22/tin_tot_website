namespace Tin_Tot_Website.Areas.Admin.Models;

public class CategoryManagementPageViewModel
{
    public IReadOnlyList<CategoryManagementItemViewModel> Categories { get; set; } = [];
    public IReadOnlyList<CategoryParentOptionViewModel> ParentOptions { get; set; } = [];
}

public class CategoryManagementItemViewModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string? Image { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
}

public class CategoryParentOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
