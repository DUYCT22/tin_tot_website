using System;
using System.Collections.Generic;
using System.Text;
using TinTot.Application.DTOs.Users;
using TinTot.Application.Interfaces.Users;

namespace TinTot.Application.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            return MapToDto(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            // mở rộng thêm method GetAll ở repository nếu cần
            throw new NotImplementedException();
        }

        private static UserDto MapToDto(Domain.Entities.User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                LoginName = user.LoginName,
                Avatar = user.Avatar,
                Role = user.Role,
                Online = user.Online,
                CreatedAt = user.CreatedAt,
                Status = user.Status
            };
        }
    }
}
