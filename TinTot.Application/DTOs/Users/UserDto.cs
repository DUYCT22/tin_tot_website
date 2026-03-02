using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinTot.Application.DTOs.Users
{
    public class UserDto
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? LoginName { get; set; }
        public string? Avatar { get; set; }

        public int Role { get; set; }
        public bool Online { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Status { get; set; }
    }
}
