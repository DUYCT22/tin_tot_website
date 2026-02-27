using System.Security.Cryptography;
using TinTot.Application.DTOs;
using TinTot.Application.Interfaces;
using TinTot.Domain.Entities;

namespace TinTot.Application.Services
{
    public class AuthService : IAuthService
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> RegisterAsync(RegisterDto dto)
        {
            var normalizedLoginName = dto.LoginName.Trim();
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

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

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = normalizedEmail,
                Phone = dto.Phone.Trim(),
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
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
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
    }
}
