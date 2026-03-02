using TinTot.Application.DTOs.Users;

namespace TinTot.Application.Interfaces.Users
{
    public interface IUserService
    {
        Task<UserDto?> GetByIdAsync(int id);
        Task<IEnumerable<UserDto>> GetAllAsync();
    }
}
