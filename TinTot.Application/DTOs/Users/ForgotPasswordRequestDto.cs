using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace TinTot.Application.DTOs.Users
{
    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = default!;
    }

    public class VerifyForgotPasswordCodeDto
    {
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = default!;

        [Required]
        [RegularExpression("^\\d{6}$")]
        public string Code { get; set; } = default!;
    }

    public class ResetForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = default!;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; } = default!;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string ConfirmPassword { get; set; } = default!;
    }
}
