using Microsoft.AspNetCore.Mvc;
using TinTot.Application.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Tin_Tot_Website.Services;
using TinTot.Application.Interfaces.Users;
using System.Security.Claims;

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
        private readonly IJwtTokenService _jwtTokenService;

        public UserController(IAuthService authService, IJwtTokenService jwtTokenService)
        {
            _authService = authService;
            _jwtTokenService = jwtTokenService;
        }
        [HttpGet("~/Dang-nhap")]
        public IActionResult LoginPage()
        {
            ViewData["InitialMode"] = "login";
            return View("~/Views/User/Login.cshtml");
        }

        [HttpGet("~/Dang-ky")]
        public IActionResult RegisterPage()
        {
            ViewData["InitialMode"] = "register";
            return View("~/Views/User/Login.cshtml");
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

        [AllowAnonymous]
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
            var token = _jwtTokenService.GenerateToken(user);
            return Ok(new
            {
                success = true,
                message = "Đăng nhập thành công",
                redirectUrl = Url.Action("Index", "Home"),
                token,
                user
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { success = false, message = "Phiên đăng nhập không hợp lệ." });
            }

            var loggedOut = await _authService.LogoutAsync(userId);
            if (!loggedOut)
            {
                return NotFound(new { success = false, message = "Không tìm thấy tài khoản người dùng." });
            }

            return Ok(new { success = true, message = "Đăng xuất thành công" });
        }
    }
}
