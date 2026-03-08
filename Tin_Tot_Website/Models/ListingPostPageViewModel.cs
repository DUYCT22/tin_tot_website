using TinTot.Domain.Entities;

namespace Tin_Tot_Website.Models
{
    public class ListingPostPageViewModel
    {
        public List<Category> ParentCategories { get; set; } = new();
    }
}
