using bakery_web_api.Interfaces.User;
using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Controllers.User;

[Route("api/[controller]")]
[ApiController]
public class ContactFormController : ControllerBase
{
    private readonly IContactForm _contactForm;

    public ContactFormController(IContactForm contactForm)
    {
        _contactForm = contactForm;
    }

    [HttpPost]
    [Route("sendMessage")]
    public async Task<IActionResult> SendMessage(ContactFormDto contactFormDto)
    {
        return await _contactForm.SendMessage(contactFormDto);
    }
}