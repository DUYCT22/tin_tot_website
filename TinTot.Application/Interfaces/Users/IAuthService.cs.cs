using TinTot.Application.DTOs.Users;

namespace TinTot.Application.Interfaces.Users
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterDto dto, AvatarUploadDto? avatarUpload = null);
        Task<LoginResultDto> LoginAsync(LoginDto dto);
        Task<bool> LogoutAsync(int userId);
        Task<bool> RequestPasswordResetCodeAsync(ForgotPasswordRequestDto dto);
        Task<bool> VerifyPasswordResetCodeAsync(VerifyForgotPasswordCodeDto dto);
        Task<(bool Success, string Message)> ResetPasswordByCodeAsync(ResetForgotPasswordDto dto);
    }
}
