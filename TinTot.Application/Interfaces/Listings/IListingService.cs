using TinTot.Application.DTOs.Listing;
using TinTot.Application.DTOs.Users;

namespace TinTot.Application.Interfaces.Listings
{
    public interface IListingService
    {
        Task<ListingDto> CreateAsync(ListingCreateDto dto, IReadOnlyCollection<AvatarUploadDto> imageUploads);
        Task<ListingDto> UpdateAsync(int id, ListingUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
