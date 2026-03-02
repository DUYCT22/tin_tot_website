using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Application.DTOs;
using TinTot.Application.DTOs.Users;
using TinTot.Application.Interfaces.Banners;
using TinTot.Application.Interfaces.Users;
using TinTot.Domain.Entities;

namespace TinTot.Application.Services
{
    public class BannerService : IBannerService
    {
        private const long MaxImageSizeInBytes = 5 * 1024 * 1024;

        private readonly IBannerRepository _bannerRepository;
        private readonly IAvatarStorageService _storageService;

        public BannerService(IBannerRepository bannerRepository, IAvatarStorageService storageService)
        {
            _bannerRepository = bannerRepository;
            _storageService = storageService;
        }

        public async Task<BannerDto> CreateAsync(BannerUpsertDto dto, AvatarUploadDto imageUpload)
        {
            ValidateLink(dto.Link);
            ValidateImage(imageUpload);

            var banner = new Banner
            {
                Link = dto.Link.Trim(),
                Status = dto.Status,
                Orders = dto.Orders,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.ActorUserId
            };
            await _bannerRepository.AddAsync(banner);
            await _bannerRepository.SaveChangesAsync();

            var extension = Path.GetExtension(imageUpload.FileName).ToLowerInvariant();
            var fileName = $"banners/{banner.Id}{extension}";

            banner.Image = await _storageService.UploadImageAsync(imageUpload.Content, fileName);

            await _bannerRepository.UpdateAsync(banner);
            await _bannerRepository.SaveChangesAsync();

            return Map(banner);
        }


        public async Task<BannerDto> UpdateAsync(int id, BannerUpsertDto dto, AvatarUploadDto? imageUpload)
        {
            var banner = await _bannerRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Banner không tồn tại.");
            ValidateLink(dto.Link);

            banner.Link = dto.Link.Trim();
            banner.Status = dto.Status;
            banner.Orders = dto.Orders;
            banner.UpdatedAt = DateTime.UtcNow;
            banner.UpdatedBy = dto.ActorUserId;

            if (imageUpload is not null)
            {
                ValidateImage(imageUpload);
                var oldPublicId = ExtractPublicIdFromCloudinaryUrl(banner.Image);

                var extension = Path.GetExtension(imageUpload.FileName).ToLowerInvariant();
                var fileName = $"banners/{banner.Id}{extension}";
                banner.Image = await _storageService.UploadImageAsync(imageUpload.Content, fileName);

                if (!string.IsNullOrWhiteSpace(oldPublicId))
                    await _storageService.DeleteImageAsync(oldPublicId);
            }


            await _bannerRepository.UpdateAsync(banner);
            await _bannerRepository.SaveChangesAsync();
            return Map(banner);
        }

        public async Task DeleteAsync(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Banner không tồn tại.");
            var oldPublicId = ExtractPublicIdFromCloudinaryUrl(banner.Image);
            await _bannerRepository.DeleteAsync(banner);
            await _bannerRepository.SaveChangesAsync();
            if (!string.IsNullOrWhiteSpace(oldPublicId))
                await _storageService.DeleteImageAsync(oldPublicId);
        }

        private static void ValidateLink(string link)
        {
            if (string.IsNullOrWhiteSpace(link))
                throw new InvalidOperationException("Link là bắt buộc.");
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
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var uploadIndex = Array.FindIndex(segments, s => s.Equals("upload", StringComparison.OrdinalIgnoreCase));
            if (uploadIndex < 0 || uploadIndex + 1 >= segments.Length) return null;
            var publicSegments = segments.Skip(uploadIndex + 1).ToList();
            if (publicSegments.Count > 0 && publicSegments[0].StartsWith("v", StringComparison.OrdinalIgnoreCase) && int.TryParse(publicSegments[0][1..], out _))
                publicSegments.RemoveAt(0);
            if (publicSegments.Count == 0) return null;
            publicSegments[^1] = Path.GetFileNameWithoutExtension(publicSegments[^1]);
            return string.Join('/', publicSegments);
        }

        private static BannerDto Map(Banner banner) => new()
        {
            Id = banner.Id,
            Link = banner.Link,
            Image = banner.Image,
            Status = banner.Status,
            Orders = banner.Orders,
            CreatedBy = banner.CreatedBy,
            UpdatedBy = banner.UpdatedBy
        };
    }
}
