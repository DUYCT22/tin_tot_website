using Microsoft.AspNetCore.Mvc;
using Tin_Tot_Website.Models;
using TinTot.Application.DTOs.Contact;
using TinTot.Application.Interfaces.Contact;

namespace Tin_Tot_Website.Controllers
{
    public class ContactController : Controller
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpGet("Lien-he")]
        public IActionResult Index()
        {
            return View(new ContactPageViewModel());
        }

        [HttpPost("Lien-he")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactPageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = new ContactRequestDto
            {
                SenderEmail = model.SenderEmail,
                FullName = model.FullName,
                Issue = model.Issue
            };

            try
            {
                await _contactService.SendContactAsync(request);
                TempData["ContactSuccess"] = "Đã gửi liên hệ thành công. Chúng tôi sẽ phản hồi sớm nhất.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Không thể gửi liên hệ lúc này: {ex.Message}");
                return View(model);
            }
        }
    }
}
