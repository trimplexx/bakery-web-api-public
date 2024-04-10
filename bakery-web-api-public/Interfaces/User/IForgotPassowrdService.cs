using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Controllers.User;

public interface IForgotPassowrdService
{
    Task<IActionResult> ForgotPassword(string email);
    Task<ActionResult> TokenVerification(TokenValidationDto token);
    Task<ActionResult> ResetPassword(string password, TokenValidationDto token);
}