using System.Security.Cryptography;
using TinTot.Application.DTOs.Users;
using TinTot.Application.Interfaces.Users;
using TinTot.Domain.Entities;

namespace TinTot.Application.Services.Users
{
    public class AuthService : IAuthService
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;
        private static readonly HashSet<string> AllowedAvatarExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".jpge"
        };
        private readonly IUserRepository _userRepository;
        private readonly IAvatarStorageService _avatarStorageService;

        public AuthService(IUserRepository userRepository, IAvatarStorageService avatarStorageService)
        {
            _userRepository = userRepository;
            _avatarStorageService = avatarStorageService;
        }

        public async Task<UserDto> RegisterAsync(RegisterDto dto, AvatarUploadDto? avatarUpload = null)
        {
            var normalizedLoginName = dto.LoginName.Trim();
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var normalizedPhone = dto.Phone.Trim();

            var existingByLoginName = await _userRepository.GetByLoginNameAsync(normalizedLoginName);
            if (existingByLoginName != null)
            {
                throw new InvalidOperationException("Tên đăng nhập đã tồn tại");
            }

            var existingByEmail = await _userRepository.GetByEmailAsync(normalizedEmail);
            if (existingByEmail != null)
            {
                throw new InvalidOperationException("Email đã tồn tại");
            }
            string? avatarUrl = null;
            if (avatarUpload is not null)
            {
                avatarUrl = await UploadAvatarAsync(avatarUpload);
            }
            var existingByPhone = await _userRepository.GetByPhoneAsync(normalizedPhone);
            if (existingByPhone != null)
            {
                throw new InvalidOperationException("Số điện thoại đã tồn tại");
            }

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = normalizedEmail,
                Phone = normalizedPhone,
                Avatar = avatarUrl,
                LoginName = normalizedLoginName,
                Password = HashPassword(dto.Password),
                Role = 0,
                Online = false,
                CreatedAt = DateTime.UtcNow,
                Status = true
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return new UserDto
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Avatar = user.Avatar,
                LoginName = user.LoginName,
                Role = user.Role,
                Online = user.Online,
                CreatedAt = user.CreatedAt,
                Status = user.Status
            };
        }

        public async Task<UserDto?> LoginAsync(LoginDto dto)
        {
            var loginName = dto.LoginName.Trim();
            var user = await _userRepository.GetByLoginNameAsync(loginName);

            if (user?.Password is null || !VerifyPassword(dto.Password, user.Password))
            {
                return null;
            }

            user.Online = true;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return ToUserDto(user);
        }

        private static UserDto ToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Avatar = user.Avatar,
                LoginName = user.LoginName,
                Role = user.Role,
                Online = user.Online,
                CreatedAt = user.CreatedAt,
                Status = user.Status
            };
        }

        private static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);

            return $"{Convert.ToHexString(salt)}:{Convert.ToHexString(hash)}";
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':', 2);
            if (parts.Length != 2)
            {
                return false;
            }

            var salt = Convert.FromHexString(parts[0]);
            var expectedHash = Convert.FromHexString(parts[1]);
            var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);

            return CryptographicOperations.FixedTimeEquals(computedHash, expectedHash);
        }
        private async Task<string> UploadAvatarAsync(AvatarUploadDto avatarUpload)
        {
            var extension = Path.GetExtension(avatarUpload.FileName).ToLowerInvariant();
            if (!AllowedAvatarExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Avatar chỉ hỗ trợ file ảnh PNG, JPG, JPEG hoặc JPGE");
            }

            var allAvatarUrls = await _userRepository.GetAllAvatarUrlsAsync();
            var nextNumber = GetNextAvatarNumber(allAvatarUrls);
            var publicId = $"User/{nextNumber}";

            return await _avatarStorageService.UploadImageAsync(avatarUpload.Content, publicId);
        }

        private static int GetNextAvatarNumber(IEnumerable<string?> avatarUrls)
        {
            var maxNumber = 0;

            foreach (var avatarUrl in avatarUrls.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var fileName = GetFileNameWithoutExtension(avatarUrl!);
                if (int.TryParse(fileName, out var current) && current > maxNumber)
                {
                    maxNumber = current;
                }
            }

            return maxNumber + 1;
        }

        private static string GetFileNameWithoutExtension(string avatarUrl)
        {
            if (Uri.TryCreate(avatarUrl, UriKind.Absolute, out var absoluteUri))
            {
                return Path.GetFileNameWithoutExtension(absoluteUri.AbsolutePath);
            }

            return Path.GetFileNameWithoutExtension(avatarUrl);
        }
    }
}
    