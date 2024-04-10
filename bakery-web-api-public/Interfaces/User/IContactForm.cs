using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Interfaces.User;

public interface IContactForm
{
    Task<IActionResult> SendMessage(ContactFormDto contactFormDto);
}