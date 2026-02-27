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
        private readonly IAuthService _authService;

        public UserController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var user = await _authService.RegisterAsync(dto);
                return CreatedAtAction(nameof(Register), user);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
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
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu" });
            }

            return Ok(user);
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
