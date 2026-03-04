using TinTot.Application.DTOs.Users;

namespace TinTot.Application.Interfaces.Users
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterDto dto, AvatarUploadDto? avatarUpload = null);
        Task<UserDto?> LoginAsync(LoginDto dto);
        Task<bool> LogoutAsync(int userId);
    }
}
