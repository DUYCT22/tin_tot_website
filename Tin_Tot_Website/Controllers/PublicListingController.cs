using Microsoft.AspNetCore.Mvc;
using Tin_Tot_Website.Models;
using Tin_Tot_Website.Services;
using TinTot.Application.Common;
using TinTot.Application.Interfaces.Listings;

namespace Tin_Tot_Website.Controllers
{
    public class PublicListingController : Controller
    {
        private readonly IPublicListingQueryService _queryService;
        private readonly IEntityKeyService _entityKeyService;

        public PublicListingController(IPublicListingQueryService queryService, IEntityKeyService entityKeyService)
        {
            _queryService = queryService;
            _entityKeyService = entityKeyService;
        }

        [HttpGet("Bai-dang/{slug}/{key}")]
        public async Task<IActionResult> Detail(string slug, string key)
        {
            var id = _entityKeyService.UnprotectId("listing", key);
            if (!id.HasValue)
            {
                return NotFound();
            }

            var detail = await _queryService.GetListingDetailAsync(id.Value);
            if (detail is null)
            {
                return NotFound();
            }

            var canonicalSlug = SlugHelper.ToSlug(detail.Title);
            if (!string.Equals(slug, canonicalSlug, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToActionPermanent(nameof(Detail), new
                {
                    slug = canonicalSlug,
                    key
                });
            }

            var relatedRatingMap = detail.RelatedListings
                .Where(x => x.UserId.HasValue)
                .GroupBy(x => x.UserId!.Value)
                .ToDictionary(g => g.Key, g => g.First().UserRatingAverage);

            var vm = new ListingDetailPageViewModel
            {
                Listing = detail,
                ListingKey = key,
                SellerKey = detail.UserId.HasValue ? _entityKeyService.ProtectId("seller", detail.UserId.Value) : string.Empty,
                RelatedSection = new ListingCardSectionViewModel
                {
                    Listings = detail.RelatedListings,
                    UserRatingAvg = relatedRatingMap,
                    EmptyMessage = "Chưa có bài đăng liên quan."
                }
            };

            return View(vm);
        }

        [HttpGet("Nguoi-ban/{slug}/{key}")]
        public async Task<IActionResult> ProfileSeller(string slug, string key)
        {
            var sellerId = _entityKeyService.UnprotectId("seller", key);
            if (!sellerId.HasValue)
            {
                return NotFound();
            }

            var profile = await _queryService.GetSellerProfileAsync(sellerId.Value);
            if (profile is null)
            {
                return NotFound();
            }

            if (!profile.Status)
            {
                return NotFound();
            }

            var canonicalSlug = SlugHelper.ToSlug(profile.SellerName);
            if (!string.Equals(slug, canonicalSlug, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToActionPermanent(nameof(ProfileSeller), new
                {
                    slug = canonicalSlug,
                    key
                });
            }

            var vm = new SellerProfilePageViewModel
            {
                Seller = profile,
                SellerKey = key,
                ActiveListingsSection = new ListingCardSectionViewModel
                {
                    Listings = profile.ActiveListings,
                    UserRatingAvg = profile.UserRatingAvg,
                    EmptyMessage = "Người bán chưa có bài đăng đang hiển thị."
                }
            };

            return View(vm);
        }
    }
}
