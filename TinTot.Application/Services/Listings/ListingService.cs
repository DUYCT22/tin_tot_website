using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Application.DTOs.Listing;
using TinTot.Application.DTOs.Users;
using TinTot.Application.Interfaces.Images;
using TinTot.Application.Interfaces.Listings;
using TinTot.Application.Common;
using TinTot.Domain.Entities;

namespace TinTot.Application.Services.Listings
{
    public class ListingService : IListingService
    {
        private readonly IListingRepository _listingRepository;
        private readonly IListingImageService _listingImageService;

        public ListingService(IListingRepository listingRepository, IListingImageService listingImageService)
        {
            _listingRepository = listingRepository;
            _listingImageService = listingImageService;
        }

        public async Task<ListingDto> CreateAsync(ListingCreateDto dto, IReadOnlyCollection<AvatarUploadDto> imageUploads)
        {
            var sanitizedDescription = HtmlContentSanitizer.Sanitize(dto.Description);
            Validate(dto.Title, sanitizedDescription, dto.Location, dto.Price);
            ValidateImageCount(imageUploads.Count);

            if (!await _listingRepository.UserExistsAsync(dto.ActorUserId))
                throw new InvalidOperationException("UserId không tồn tại.");

            if (!await _listingRepository.CategoryExistsAsync(dto.CategoryId))
                throw new InvalidOperationException("CategoryId không tồn tại.");

            var listing = new Listing
            {
                UserId = dto.ActorUserId,
                CategoryId = dto.CategoryId,
                Title = dto.Title.Trim(),
                Description = sanitizedDescription,
                Price = dto.Price,
                Location = dto.Location.Trim(),
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow
            };

            await _listingRepository.AddAsync(listing);
            await _listingRepository.SaveChangesAsync();

            foreach (var imageUpload in imageUploads)
            {
                await _listingImageService.CreateAsync(listing.Id, imageUpload);
            }

            return Map(listing);
        }

        public async Task<ListingDto> UpdateAsync(int id, ListingUpdateDto dto)
        {
            var listing = await _listingRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Bài đăng không tồn tại.");
            var sanitizedDescription = HtmlContentSanitizer.Sanitize(dto.Description);
            Validate(dto.Title, sanitizedDescription, dto.Location, dto.Price);

            if (!await _listingRepository.CategoryExistsAsync(dto.CategoryId))
                throw new InvalidOperationException("CategoryId không tồn tại.");

            listing.CategoryId = dto.CategoryId;
            listing.Title = dto.Title.Trim();
            listing.Description = sanitizedDescription;
            listing.Price = dto.Price;
            listing.Location = dto.Location.Trim();
            listing.Status = dto.Status;
            listing.UpdatedAt = DateTime.UtcNow;

            await _listingRepository.UpdateAsync(listing);
            await _listingRepository.SaveChangesAsync();

            return Map(listing);
        }

        public async Task DeleteAsync(int id)
        {
            var listing = await _listingRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Bài đăng không tồn tại.");
            await _listingImageService.DeleteByListingIdAsync(id);

            await _listingRepository.DeleteAsync(listing);
            await _listingRepository.SaveChangesAsync();
        }

        private static void Validate(string title, string description, string location, decimal price)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new InvalidOperationException("Title là bắt buộc.");
            if (string.IsNullOrWhiteSpace(HtmlContentSanitizer.ToPlainText(description)))
                throw new InvalidOperationException("Description là bắt buộc.");
            if (string.IsNullOrWhiteSpace(location))
                throw new InvalidOperationException("Location là bắt buộc.");
            if (price < 0)
                throw new InvalidOperationException("Price không hợp lệ.");
        }

        private static void ValidateImageCount(int imageCount)
        {
            if (imageCount < 1)
                throw new InvalidOperationException("Bài đăng phải có tối thiểu 1 hình ảnh.");

            if (imageCount > 5)
                throw new InvalidOperationException("Bài đăng chỉ được tối đa 5 hình ảnh.");
        }

        private static ListingDto Map(Listing listing) => new()
        {
            Id = listing.Id,
            UserId = listing.UserId,
            CategoryId = listing.CategoryId,
            Title = listing.Title,
            Description = listing.Description,
            Price = listing.Price,
            Location = listing.Location,
            Status = listing.Status,
            CreatedAt = listing.CreatedAt,
            UpdatedAt = listing.UpdatedAt
        };
    }
}
