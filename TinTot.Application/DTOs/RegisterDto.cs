using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinTot.Application.DTOs
{
    public class RegisterDto
    {
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string LoginName { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
