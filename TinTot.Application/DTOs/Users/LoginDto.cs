using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinTot.Application.DTOs.Users
{
    public class LoginDto
    {
        [Required]
        [StringLength(100)]
        public string LoginName { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = default!;

        public string? RecaptchaToken { get; set; }
    }
}
