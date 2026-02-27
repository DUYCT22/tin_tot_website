
using TinTot.Application.DTOs;

namespace TinTot.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<UserDto>> GetAllAsync();
    }
}
