using TinTot.Application.DTOs.Listing;
using TinTot.Application.DTOs.Users;
using TinTot.Application.Interfaces.Images;
using TinTot.Application.Interfaces.Users;
using TinTot.Domain.Entities;

namespace TinTot.Application.Services.Listings
{
    public class ListingImageService : IListingImageService
    {
        private const long MaxImageSizeInBytes = 5 * 1024 * 1024;
        private readonly IListingImageRepository _listingImageRepository;
        private readonly IAvatarStorageService _storageService;

        public ListingImageService(IListingImageRepository listingImageRepository, IAvatarStorageService storageService)
        {
            _listingImageRepository = listingImageRepository;
            _storageService = storageService;
        }

        public async Task<ListingImageDto> CreateAsync(int listingId, AvatarUploadDto imageUpload)
        {
            ValidateImage(imageUpload);

            if (!await _listingImageRepository.ListingExistsAsync(listingId))
                throw new InvalidOperationException("ListingId không tồn tại.");

            var imageCount = await _listingImageRepository.CountByListingIdAsync(listingId);
            if (imageCount >= 5)
                throw new InvalidOperationException("Mỗi bài đăng chỉ được tối đa 5 ảnh.");

            var image = new Image { ListingId = listingId };
            await _listingImageRepository.AddAsync(image);
            await _listingImageRepository.SaveChangesAsync();

            var extension = Path.GetExtension(imageUpload.FileName).ToLowerInvariant();
            var fileName = $"listings/{listingId}/{image.Id}{extension}";
            image.ImageUrl = await _storageService.UploadImageAsync(imageUpload.Content, fileName);

            await _listingImageRepository.UpdateAsync(image);
            await _listingImageRepository.SaveChangesAsync();

            return Map(image);
        }

        public async Task<ListingImageDto> UpdateAsync(int id, int listingId, AvatarUploadDto imageUpload)
        {
            ValidateImage(imageUpload);

            if (!await _listingImageRepository.ListingExistsAsync(listingId))
                throw new InvalidOperationException("ListingId không tồn tại.");

            var image = await _listingImageRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Hình ảnh không tồn tại.");
            if (image.ListingId != listingId)
                throw new InvalidOperationException("Hình ảnh không thuộc bài đăng đã chọn.");

            var oldPublicId = ExtractPublicIdFromCloudinaryUrl(image.ImageUrl);
            var extension = Path.GetExtension(imageUpload.FileName).ToLowerInvariant();
            var fileName = $"listings/{listingId}/{image.Id}{extension}";
            image.ImageUrl = await _storageService.UploadImageAsync(imageUpload.Content, fileName);

            await _listingImageRepository.UpdateAsync(image);
            await _listingImageRepository.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(oldPublicId))
                await _storageService.DeleteImageAsync(oldPublicId);

            return Map(image);
        }

        public async Task DeleteByListingIdAsync(int listingId)
        {
            if (!await _listingImageRepository.ListingExistsAsync(listingId))
                throw new KeyNotFoundException("Bài đăng không tồn tại.");

            var images = await _listingImageRepository.GetByListingIdAsync(listingId);
            foreach (var image in images)
            {
                var oldPublicId = ExtractPublicIdFromCloudinaryUrl(image.ImageUrl);
                if (!string.IsNullOrWhiteSpace(oldPublicId))
                    await _storageService.DeleteImageAsync(oldPublicId);
            }

            await _listingImageRepository.DeleteRangeAsync(images);
            await _listingImageRepository.SaveChangesAsync();
        }

        private static void ValidateImage(AvatarUploadDto imageUpload)
        {
            if (imageUpload.Content.Length > MaxImageSizeInBytes)
                throw new InvalidOperationException("Ảnh vượt quá 5MB.");

            var ext = Path.GetExtension(imageUpload.FileName).ToLowerInvariant();
            if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp" or ".gif"))
                throw new InvalidOperationException("Định dạng ảnh không hợp lệ.");
        }

        private static string? ExtractPublicIdFromCloudinaryUrl(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return null;
            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri)) return null;
            var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            var uploadIndex = Array.FindIndex(segments, x => x.Equals("upload", StringComparison.OrdinalIgnoreCase));
            if (uploadIndex < 0 || uploadIndex >= segments.Length - 1) return null;
            var publicSegments = segments.Skip(uploadIndex + 1).ToList();
            if (publicSegments.Count > 0 && publicSegments[0].StartsWith("v", StringComparison.OrdinalIgnoreCase) && int.TryParse(publicSegments[0][1..], out _))
                publicSegments.RemoveAt(0);
            if (publicSegments.Count == 0) return null;
            publicSegments[^1] = Path.GetFileNameWithoutExtension(publicSegments[^1]);
            return string.Join('/', publicSegments);
        }

        private static ListingImageDto Map(Image image) => new()
        {
            Id = image.Id,
            ListingId = image.ListingId,
            ImageUrl = image.ImageUrl
        };
    }
}
