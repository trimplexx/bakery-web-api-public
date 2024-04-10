using bakery_web_api.Interfaces.Common;
using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Controllers.Common;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        return await _authService.Register(registerDto);
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(LoginDto loginRequest)
    {
        return await _authService.Login(loginRequest);
    }

    [HttpPost]
    [Route("logout")]
    public async Task<IActionResult> Logout(SessionTokens token)
    {
        return await _authService.Logout(token);
    }

    [HttpPost]
    [Route("refresh")]
    public async Task<ActionResult> Refresh([FromBody] SessionTokens refreshToken)
    {
        return await _authService.Refresh(refreshToken);
    }

    [HttpGet]
    [Route("gmailLogin")]
    public async Task<IActionResult> GmailLogin()
    {
        return await _authService.GmailLogin();
    }

    [HttpGet]
    [Route("authorizeGmailLogin")]
    public async Task<IActionResult> AuthorizeGmailLogin(string code)
    {
        return await _authService.AuthorizeGmailLogin(code);
    }

    [HttpGet]
    [Route("facebookLogin")]
    public async Task<IActionResult> FacebookLogin()
    {
        return await _authService.FacebookLogin();
    }

    [HttpGet]
    [Route("authorizeFacebookLogin")]
    public async Task<IActionResult> AuthorizeFacebookLogin(string code)
    {
        return await _authService.AuthorizeFacebookLogin(code);
    }
}