using Microsoft.AspNetCore.Mvc;
using Tin_Tot_Website.Models;
using TinTot.Application.DTOs;
using TinTot.Application.Interfaces;

namespace Tin_Tot_Website.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : Controller
    {
        private const long MaxAvatarSizeInBytes = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedAvatarContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif"
        };
        private readonly IAuthService _authService;

        public UserController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpGet("~/login")]
        public IActionResult LoginPage()
        {
            return View("~/Views/User/Login.cshtml");
        }

        [HttpGet("~/register")]
        public IActionResult RegisterPage()
        {
            return View("~/Views/User/Register.cshtml");
        }

        [HttpPost("register")]
        [RequestSizeLimit(10_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10_000_000)]
        public async Task<IActionResult> Register([FromForm] RegisterDto dto, IFormFile? avatar)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            if (avatar is not null && avatar.Length > 0)
            {
                if (avatar.Length > MaxAvatarSizeInBytes)
                {
                    return BadRequest(new { success = false, message = "Ảnh đại diện vượt quá 5MB." });
                }

                if (!AllowedAvatarContentTypes.Contains(avatar.ContentType))
                {
                    return BadRequest(new { success = false, message = "Định dạng ảnh không hợp lệ." });
                }
            }

            try
            {
                AvatarUploadDto? avatarUpload = null;

                if (avatar is not null && avatar.Length > 0)
                {
                    await using var buffer = new MemoryStream();
                    await avatar.CopyToAsync(buffer);

                    avatarUpload = new AvatarUploadDto
                    {
                        FileName = avatar.FileName,
                        Content = new MemoryStream(buffer.ToArray())
                    };
                }

                var user = await _authService.RegisterAsync(dto, avatarUpload);
                return Ok(new
                {
                    success = true,
                    message = "Đăng ký thành công",
                    redirectUrl = Url.Action(nameof(LoginPage), "User"),
                    user
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Có lỗi trong quá trình xử lý ảnh đại diện. Vui lòng thử lại." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var user = await _authService.LoginAsync(dto);
            if (user is null)
            {
                return Unauthorized(new { success = false, message = "Sai tài khoản hoặc mật khẩu" });
            }

            return Ok(new
            {
                success = true,
                message = $"Xin chào {user.FullName ?? user.LoginName}",
                redirectUrl = Url.Action("Index", "Home"),
                user
            });
        }

        
    }
}
