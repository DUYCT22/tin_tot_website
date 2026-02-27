using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinTot.Application.DTOs
{
    public class LoginDto
    {
        public string LoginName { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
