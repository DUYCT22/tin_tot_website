using Microsoft.AspNetCore.Mvc;
using Tin_Tot_Website.Models;

namespace Tin_Tot_Website.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : Controller
    {
        
        public IActionResult Index()
        {
            return View();
        }
    }
}
