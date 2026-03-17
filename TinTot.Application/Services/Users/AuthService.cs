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
        private static readonly ConcurrentDictionary<int, FailedAttemptState> InMemoryFailedAttempts = new();
        private static readonly ConcurrentDictionary<int, DateTime> InMemoryLockoutUntil = new();
        private static readonly ConcurrentDictionary<string, PasswordResetCode> InMemoryRegisterVerificationCodes = new(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, DateTime> InMemoryRegisterCodeCooldownUntil = new(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> AllowedAvatarExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".jpge"
        };
        private readonly IUserRepository _userRepository;
        private readonly IAvatarStorageService _avatarStorageService;

        private readonly IPasswordResetRepository _passwordResetRepository;
        private readonly IPasswordResetEmailSender _passwordResetEmailSender;

        public AuthService(
            IUserRepository userRepository,
            IAvatarStorageService avatarStorageService,
            IPasswordResetRepository passwordResetRepository,
            IPasswordResetEmailSender passwordResetEmailSender)
        {
            _userRepository = userRepository;
            _avatarStorageService = avatarStorageService;
            _passwordResetRepository = passwordResetRepository;
            _passwordResetEmailSender = passwordResetEmailSender;
        }

        public async Task<UserDto> RegisterAsync(RegisterDto dto, AvatarUploadDto? avatarUpload = null)
        {
            var normalizedLoginName = dto.LoginName.Trim();
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var normalizedPhone = dto.Phone.Trim();

            var isEmailVerified = await IsRegisterEmailVerifiedAsync(normalizedEmail);
            if (!isEmailVerified)
            {
                throw new InvalidOperationException("Vui lòng xác thực email trước khi đăng ký.");
            }
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
            try
            {
                await _userRepository.SaveChangesAsync();
            }
            catch
            {
                if (await _userRepository.GetByEmailAsync(normalizedEmail) is not null)
                {
                    throw new InvalidOperationException("Email đã tồn tại");
                }

                if (await _userRepository.GetByLoginNameAsync(normalizedLoginName) is not null)
                {
                    throw new InvalidOperationException("Tên đăng nhập đã tồn tại");
                }

                if (await _userRepository.GetByPhoneAsync(normalizedPhone) is not null)
                {
                    throw new InvalidOperationException("Số điện thoại đã tồn tại");
                }

                throw;
            }
            InMemoryRegisterVerificationCodes.TryRemove(normalizedEmail, out _);
            InMemoryRegisterCodeCooldownUntil.TryRemove(normalizedEmail, out _);
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
        public async Task<bool> RequestPasswordResetCodeAsync(ForgotPasswordRequestDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);
            if (user is null)
            {
                return false;
            }

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var passwordResetCode = new PasswordResetCode
            {
                Email = normalizedEmail,
                Code = code,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
                IsVerified = false
            };

            await _passwordResetRepository.UpsertAsync(passwordResetCode);
            await _passwordResetEmailSender.SendResetCodeAsync(normalizedEmail, code);

            return true;
        }

        public async Task<bool> VerifyPasswordResetCodeAsync(VerifyForgotPasswordCodeDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var passwordResetCode = await _passwordResetRepository.GetByEmailAsync(normalizedEmail);
            if (passwordResetCode is null)
            {
                return false;
            }

            var isValidCode = passwordResetCode.Code == dto.Code
                && passwordResetCode.ExpiresAtUtc >= DateTime.UtcNow;
            if (!isValidCode)
            {
                return false;
            }

            passwordResetCode.IsVerified = true;
            await _passwordResetRepository.UpsertAsync(passwordResetCode);
            return true;
        }

        public async Task<(bool Success, string Message)> ResetPasswordByCodeAsync(ResetForgotPasswordDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            if (dto.NewPassword != dto.ConfirmPassword)
            {
                return (false, "Mật khẩu xác nhận không khớp.");
            }

            var passwordResetCode = await _passwordResetRepository.GetByEmailAsync(normalizedEmail);
            if (passwordResetCode is null
                || !passwordResetCode.IsVerified
                || passwordResetCode.ExpiresAtUtc < DateTime.UtcNow)
            {
                return (false, "Phiên đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
            }

            var user = await _userRepository.GetByEmailAsync(normalizedEmail);
            if (user is null)
            {
                return (false, "Không tìm thấy người dùng.");
            }

            if (user.Password is not null && VerifyPassword(dto.NewPassword, user.Password))
            {
                return (false, "Không được sử dụng mật khẩu cũ.");
            }

            user.Password = HashPassword(dto.NewPassword);
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
            await _passwordResetRepository.RemoveAsync(normalizedEmail);

            return (true, "Đặt lại mật khẩu thành công.");
        }
        public async Task<(bool Success, string Message, int? RetryAfterSeconds)> RequestRegisterVerificationCodeAsync(ForgotPasswordRequestDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var existingByEmail = await _userRepository.GetByEmailAsync(normalizedEmail);
            if (existingByEmail is not null)
            {
                return (false, "Email đã tồn tại trong hệ thống.", null);
            }

            if (InMemoryRegisterCodeCooldownUntil.TryGetValue(normalizedEmail, out var cooldownUntil)
                && cooldownUntil > DateTime.UtcNow)
            {
                var remaining = Math.Max(1, (int)Math.Ceiling((cooldownUntil - DateTime.UtcNow).TotalSeconds));
                return (false, $"Vui lòng chờ {remaining}s để gửi lại mã xác thực.", remaining);
            }

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            InMemoryRegisterVerificationCodes[normalizedEmail] = new PasswordResetCode
            {
                Email = normalizedEmail,
                Code = code,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
                IsVerified = false
            };
            InMemoryRegisterCodeCooldownUntil[normalizedEmail] = DateTime.UtcNow.AddSeconds(30);

            await _passwordResetEmailSender.SendResetCodeAsync(normalizedEmail, code);
            return (true, "Mã xác thực đã được gửi về email của bạn.", 30);
        }

        public Task<bool> VerifyRegisterVerificationCodeAsync(VerifyForgotPasswordCodeDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            if (!InMemoryRegisterVerificationCodes.TryGetValue(normalizedEmail, out var registerVerificationCode))
            {
                return Task.FromResult(false);
            }

            var isValidCode = registerVerificationCode.Code == dto.Code
                && registerVerificationCode.ExpiresAtUtc >= DateTime.UtcNow;

            if (!isValidCode)
            {
                return Task.FromResult(false);
            }

            registerVerificationCode.IsVerified = true;
            InMemoryRegisterVerificationCodes[normalizedEmail] = registerVerificationCode;
            return Task.FromResult(true);
        }

        public Task<bool> IsRegisterEmailVerifiedAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            if (!InMemoryRegisterVerificationCodes.TryGetValue(normalizedEmail, out var registerVerificationCode))
            {
                return Task.FromResult(false);
            }

            var isVerified = registerVerificationCode.IsVerified
                && registerVerificationCode.ExpiresAtUtc >= DateTime.UtcNow;

            return Task.FromResult(isVerified);
        }
        private async Task<int?> TryAutoUnlockAsync(User user)
        {
            DateTime? lockoutUntil = null;
            if (InMemoryLockoutUntil.TryGetValue(user.Id, out var inMemoryLockoutUntil))
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
            var now = DateTime.UtcNow;
            var state = InMemoryFailedAttempts.AddOrUpdate(
                userId,
                _ => new FailedAttemptState(1, now),
                (_, existing) =>
                {
                    if (now - existing.WindowStart > FailedAttemptsWindow)
                    {
                        return new FailedAttemptState(1, now);
                    }

                    return existing with { Count = existing.Count + 1 };
                });

            return state.Count;
        }

        private async Task ClearLockoutStateAsync(int userId)
        {
            InMemoryFailedAttempts.TryRemove(userId, out _);
            InMemoryLockoutUntil.TryRemove(userId, out _);
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
        private readonly record struct FailedAttemptState(int Count, DateTime WindowStart);
    }
}
    