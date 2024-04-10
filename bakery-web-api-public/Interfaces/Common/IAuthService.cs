using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Interfaces.Common;

public interface IAuthService
{
    Task<IActionResult> Register(RegisterDto registerDto);
    Task<IActionResult> Login(LoginDto loginDto);
    Task<IActionResult> GmailLogin();
    Task<IActionResult> AuthorizeGmailLogin(string code);
    Task<IActionResult> FacebookLogin();
    Task<IActionResult> AuthorizeFacebookLogin(string code);
    Task<ActionResult> Refresh(SessionTokens refreshToken);
    Task<IActionResult> Logout(SessionTokens token);
    string GenerateJwtToken(Models.Database.User user);
}