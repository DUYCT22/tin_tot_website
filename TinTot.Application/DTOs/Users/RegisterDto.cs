using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinTot.Application.DTOs.Users
{
    public class RegisterDto
    {
        [Required]
        [StringLength(255)]
        public string FullName { get; set; } = default!;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = default!;

        [Required]
        [Phone]
        [StringLength(50)]
        public string Phone { get; set; } = default!;

        [Required]
        [StringLength(100, MinimumLength = 4)]
        public string LoginName { get; set; } = default!;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = default!;

        public string? RecaptchaToken { get; set; }
    }
}
