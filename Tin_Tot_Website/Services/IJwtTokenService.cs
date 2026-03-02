using TinTot.Application.DTOs.Users;

namespace Tin_Tot_Website.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(UserDto user);
    }
}
