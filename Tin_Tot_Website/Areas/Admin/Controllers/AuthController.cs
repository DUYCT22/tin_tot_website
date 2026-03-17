using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TinTot.Application.DTOs.Users;
using TinTot.Application.Interfaces.Users;
using Tin_Tot_Website.Services;

namespace Tin_Tot_Website.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin")]
public class AuthController : Controller
{
    private static readonly HashSet<int> AllowedAdminRoles = new() { 1, 2, 3 };
    private readonly IAuthService _authService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(IAuthService authService, IJwtTokenService jwtTokenService)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
    }

    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login() => View();

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (!result.Success || result.User is null)
        {
            return Unauthorized(new { success = false, message = result.Message });
        }

        if (!AllowedAdminRoles.Contains(result.User.Role))
        {
            return Forbid();
        }

        var token = _jwtTokenService.GenerateToken(result.User);
        Response.Cookies.Append("tin_tot_access_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(12)
        });

        return Ok(new { success = true, redirectUrl = Url.Action("Index", "Dashboard", new { area = "Admin" }) });
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("tin_tot_access_token");
        return Ok(new { success = true, redirectUrl = Url.Action(nameof(Login), new { area = "Admin" }) });
    }
}
