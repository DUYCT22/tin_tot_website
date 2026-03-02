using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Application.DTOs;
using TinTot.Application.DTOs.Users;
using TinTot.Application.Interfaces.Categories;
using TinTot.Application.Interfaces.Users;
using TinTot.Domain.Entities;

namespace TinTot.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private const long MaxImageSizeInBytes = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };

        private readonly ICategoryRepository _categoryRepository;
        private readonly IAvatarStorageService _storageService;

        public CategoryService(ICategoryRepository categoryRepository, IAvatarStorageService storageService)
        {
            _categoryRepository = categoryRepository;
            _storageService = storageService;
        }

        public async Task<CategoryDto> CreateAsync(CategoryUpsertDto dto, AvatarUploadDto? imageUpload)
        {
            ValidateName(dto.Name);

            if (dto.ParentId.HasValue && !await _categoryRepository.ExistsAsync(dto.ParentId.Value))
                throw new InvalidOperationException("ParentId không tồn tại.");

            var category = new Category
            {
                Name = dto.Name.Trim(),
                ParentId = dto.ParentId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.ActorUserId
            };

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            if (imageUpload is not null)
            {
                ValidateImage(imageUpload);
                var extension = Path.GetExtension(imageUpload.FileName).ToLowerInvariant();
                var fileName = $"categories/{category.Id}{extension}";
                category.Image = await _storageService.UploadImageAsync(imageUpload.Content, fileName);

                await _categoryRepository.UpdateAsync(category);
                await _categoryRepository.SaveChangesAsync();
            }

            return Map(category);
        }


        public async Task<CategoryDto> UpdateAsync(int id, CategoryUpsertDto dto, AvatarUploadDto? imageUpload)
        {
            var category = await _categoryRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Category không tồn tại.");
            ValidateName(dto.Name);

            if (dto.ParentId == id)
                throw new InvalidOperationException("Category không thể là cha của chính nó.");

            if (dto.ParentId.HasValue && !await _categoryRepository.ExistsAsync(dto.ParentId.Value))
                throw new InvalidOperationException("ParentId không tồn tại.");

            if (category.ParentId is null && dto.ParentId.HasValue && await _categoryRepository.HasChildrenAsync(id))
                throw new InvalidOperationException("Không thể chuyển category cha thành category con khi đang có category phụ thuộc.");

            category.Name = dto.Name.Trim();
            category.ParentId = dto.ParentId;
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedBy = dto.ActorUserId;

            if (imageUpload is not null)
            {
                ValidateImage(imageUpload);
                var oldPublicId = ExtractPublicIdFromCloudinaryUrl(category.Image);

                var extension = Path.GetExtension(imageUpload.FileName).ToLowerInvariant();
                var fileName = $"categories/{category.Id}{extension}";
                category.Image = await _storageService.UploadImageAsync(imageUpload.Content, fileName);

                if (!string.IsNullOrWhiteSpace(oldPublicId))
                    await _storageService.DeleteImageAsync(oldPublicId);
            }


            await _categoryRepository.UpdateAsync(category);
            await _categoryRepository.SaveChangesAsync();
            return Map(category);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Category không tồn tại.");
            if (await _categoryRepository.HasChildrenAsync(id))
                throw new InvalidOperationException("Không thể xóa category vì có category khác đang phụ thuộc.");

            var oldPublicId = ExtractPublicIdFromCloudinaryUrl(category.Image);
            await _categoryRepository.DeleteAsync(category);
            await _categoryRepository.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(oldPublicId))
                await _storageService.DeleteImageAsync(oldPublicId);
        }

        private async Task<string?> UploadIfAnyAsync(AvatarUploadDto? imageUpload, string folder)
        {
            if (imageUpload is null) return null;
            ValidateImage(imageUpload);
            return await _storageService.UploadImageAsync(imageUpload.Content, $"{folder}/{Guid.NewGuid():N}");
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Name là bắt buộc.");
        }

        private static void ValidateImage(AvatarUploadDto imageUpload)
        {
            var contentType = GetContentTypeFromExtension(Path.GetExtension(imageUpload.FileName));
            if (imageUpload.Content.Length > MaxImageSizeInBytes)
                throw new InvalidOperationException("Ảnh vượt quá 5MB.");
            if (string.IsNullOrWhiteSpace(contentType) || !AllowedImageContentTypes.Contains(contentType))
                throw new InvalidOperationException("Định dạng ảnh không hợp lệ.");
        }

        private static string? GetContentTypeFromExtension(string ext) => ext.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => null
        };

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

        private static CategoryDto Map(Category category) => new()
        {
            Id = category.Id,
            Name = category.Name,
            ParentId = category.ParentId,
            Image = category.Image,
            CreatedBy = category.CreatedBy,
            UpdatedBy = category.UpdatedBy
        };
    }
}
