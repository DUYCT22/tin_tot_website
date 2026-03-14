using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinTot.Application.DTOs.Users
{
    public class LoginResultDto
    {
        public bool Success { get; set; }
        public bool IsLocked { get; set; }
        public int? RetryAfterSeconds { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserDto? User { get; set; }
    }
}
