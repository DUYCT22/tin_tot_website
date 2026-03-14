using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
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
        private const int MaxFailedLoginAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan FailedAttemptsWindow = TimeSpan.FromMinutes(10);
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;
        private static readonly ConcurrentDictionary<int, int> InMemoryFailedAttempts = new();
        private static readonly ConcurrentDictionary<int, DateTime> InMemoryLockoutUntil = new();
        private static readonly HashSet<string> AllowedAvatarExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".jpge"
        };
        private readonly IUserRepository _userRepository;
        private readonly IAvatarStorageService _avatarStorageService;
        private readonly IDistributedCache _cache;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepository, IAvatarStorageService avatarStorageService, IDistributedCache cache, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _avatarStorageService = avatarStorageService;
            _cache = cache;
            _logger = logger;
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

        public async Task<LoginResultDto> LoginAsync(LoginDto dto)
        {
            var loginName = dto.LoginName.Trim();
            var user = await _userRepository.GetByLoginNameAsync(loginName);
            if (user is not null)
            {
                var unlockRemaining = await TryAutoUnlockAsync(user);
                if (unlockRemaining.HasValue)
                {
                    return new LoginResultDto
                    {
                        Success = false,
                        IsLocked = true,
                        RetryAfterSeconds = unlockRemaining.Value,
                        Message = $"Tài khoản đã bị khóa tạm thời. Vui lòng thử lại sau {unlockRemaining.Value} giây."
                    };
                }

                if (!user.Status)
                {
                    return new LoginResultDto
                    {
                        Success = false,
                        Message = "Tài khoản của bạn đang bị khóa."
                    };
                }
            }

            if (user?.Password is null || !VerifyPassword(dto.Password, user.Password))
            {
                if (user is not null)
                {
                    var failedCount = await IncrementFailedAttemptsAsync(user.Id);
                    if (failedCount >= MaxFailedLoginAttempts)
                    {
                        user.Status = false;
                        user.Online = false;
                        await _userRepository.UpdateAsync(user);
                        await _userRepository.SaveChangesAsync();
                        var lockoutUntil = DateTime.UtcNow.Add(LockoutDuration);
                        InMemoryLockoutUntil[user.Id] = lockoutUntil;
                        await SetCacheStringSafeAsync(GetLockoutKey(user.Id), DateTime.UtcNow.Add(LockoutDuration).ToString("O"), new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = LockoutDuration
                        });

                        return new LoginResultDto
                        {
                            Success = false,
                            IsLocked = true,
                            RetryAfterSeconds = (int)LockoutDuration.TotalSeconds,
                            Message = "Bạn đã nhập sai mật khẩu quá 5 lần. Tài khoản bị khóa tạm thời trong 2 phút."
                        };
                    }
                }

                return new LoginResultDto
                {
                    Success = false,
                    Message = "Sai tài khoản hoặc mật khẩu"
                };
            }

            user.Online = true;
            user.Status = true;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
            await ClearLockoutStateAsync(user.Id);
            return new LoginResultDto
            {
                Success = true,
                User = ToUserDto(user),
                Message = "Đăng nhập thành công"
            };
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
            {
                return false;
            }

            user.Online = false;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }
        private async Task<int?> TryAutoUnlockAsync(User user)
        {
            DateTime? lockoutUntil = null;
            var lockoutRaw = await GetCacheStringSafeAsync(GetLockoutKey(user.Id));

            if (!string.IsNullOrWhiteSpace(lockoutRaw)
                && DateTime.TryParse(lockoutRaw, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsedLockoutUntil))
            {
                lockoutUntil = parsedLockoutUntil;
                InMemoryLockoutUntil[user.Id] = parsedLockoutUntil;
            }
            else if (InMemoryLockoutUntil.TryGetValue(user.Id, out var inMemoryLockoutUntil))
            {
                lockoutUntil = inMemoryLockoutUntil;
            }

            if (!lockoutUntil.HasValue)
            {
                return null;
            }

            var remaining = lockoutUntil.Value - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                user.Status = true;
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
                await ClearLockoutStateAsync(user.Id);
                return null;
            }

            return Math.Max(1, (int)Math.Ceiling(remaining.TotalSeconds));
        }

        private async Task<int> IncrementFailedAttemptsAsync(int userId)
        {
            var current = InMemoryFailedAttempts.AddOrUpdate(userId, 1, (_, value) => value + 1);
            var key = GetFailedAttemptsKey(userId);

            await SetCacheStringSafeAsync(key, current.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = FailedAttemptsWindow
            });

            return current;
        }

        private async Task ClearLockoutStateAsync(int userId)
        {
            InMemoryFailedAttempts.TryRemove(userId, out _);
            InMemoryLockoutUntil.TryRemove(userId, out _);
            await RemoveCacheKeySafeAsync(GetFailedAttemptsKey(userId));
            await RemoveCacheKeySafeAsync(GetLockoutKey(userId));
        }

        private async Task<string?> GetCacheStringSafeAsync(string key)
        {
            try
            {
                return await _cache.GetStringAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cannot read cache key {CacheKey}. Continue without lockout cache.", key);
                return null;
            }
        }

        private async Task SetCacheStringSafeAsync(string key, string value, DistributedCacheEntryOptions options)
        {
            try
            {
                await _cache.SetStringAsync(key, value, options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cannot write cache key {CacheKey}. Continue without lockout cache.", key);
            }
        }

        private async Task RemoveCacheKeySafeAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cannot remove cache key {CacheKey}. Continue without lockout cache.", key);
            }
        }

        private static string GetFailedAttemptsKey(int userId) => $"auth:failed:{userId}";
        private static string GetLockoutKey(int userId) => $"auth:lockout:{userId}";
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
    