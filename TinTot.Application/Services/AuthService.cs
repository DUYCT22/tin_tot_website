using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Application.DTOs;
using TinTot.Application.Interfaces;
using TinTot.Domain.Entities;

namespace TinTot.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> RegisterAsync(RegisterDto dto)
        {
            var existing = await _userRepository.GetByLoginNameAsync(dto.LoginName);
            if (existing != null)
                throw new Exception("Login name already exists");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                LoginName = dto.LoginName,
                Password = dto.Password, // sau này phải hash
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
            var user = await _userRepository.GetByLoginNameAsync(dto.LoginName);

            if (user == null || user.Password != dto.Password)
                return null;

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
    }
}
