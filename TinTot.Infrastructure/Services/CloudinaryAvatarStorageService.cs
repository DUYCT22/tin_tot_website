using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using TinTot.Application.Interfaces.Users;

namespace TinTot.Infrastructure.Services
{
    public class CloudinaryAvatarStorageService : IAvatarStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryAvatarStorageService(IConfiguration configuration)
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
            {
                throw new InvalidOperationException("Thiếu cấu hình Cloudinary");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(Stream stream, string publicId)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription("image", stream),
                PublicId = publicId,
                UseFilename = false,
                UniqueFilename = false,
                Overwrite = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.Error is not null)
            {
                throw new InvalidOperationException($"Upload ảnh thất bại: {uploadResult.Error.Message}");
            }

            return uploadResult.SecureUrl?.ToString()
                ?? throw new InvalidOperationException("Không lấy được URL ảnh từ Cloudinary");
        }

        public async Task DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return;
            }

            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);
            if (deletionResult.Error is not null)
            {
                throw new InvalidOperationException($"Xóa ảnh thất bại: {deletionResult.Error.Message}");
            }
        }
    }
}
