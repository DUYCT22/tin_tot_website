using TinTot.Application.DTOs.Listing;
using TinTot.Application.DTOs.Users;

namespace TinTot.Application.Interfaces.Images
{
    public interface IListingImageService
    {
        Task<ListingImageDto> CreateAsync(int listingId, AvatarUploadDto imageUpload);
        Task<ListingImageDto> UpdateAsync(int id, int listingId, AvatarUploadDto imageUpload);
        Task DeleteByListingIdAsync(int listingId);
    }
}
