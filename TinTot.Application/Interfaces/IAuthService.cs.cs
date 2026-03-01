using TinTot.Application.DTOs;

namespace TinTot.Application.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterDto dto, AvatarUploadDto? avatarUpload = null);
        Task<UserDto?> LoginAsync(LoginDto dto);
    }
}
