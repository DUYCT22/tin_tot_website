using System.ComponentModel.DataAnnotations;

namespace TinTot.Application.DTOs.Contact
{
    public class ContactRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string SenderEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(120, ErrorMessage = "Họ và tên tối đa 120 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập vấn đề cần liên hệ")]
        [StringLength(4000, ErrorMessage = "Nội dung tối đa 4000 ký tự")]
        public string Issue { get; set; } = string.Empty;
    }
}
