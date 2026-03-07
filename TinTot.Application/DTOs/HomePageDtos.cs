using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinTot.Application.DTOs
{
    public class HomePageDto
    {
        public List<BannerDto> Banners { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public List<HomeListingDto> Listings { get; set; } = new();
        public Dictionary<int, decimal> UserRatingAvg { get; set; } = new();
    }

    public class HomeListingDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? Title { get; set; }
        public decimal? Price { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UserAvatar { get; set; }
        public string? FirstImageUrl { get; set; }
        public decimal UserRatingAverage { get; set; }
    }
}
