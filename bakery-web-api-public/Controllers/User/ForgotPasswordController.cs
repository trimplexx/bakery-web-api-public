using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Controllers.User;

[Route("api/[controller]")]
[ApiController]
public class ForgotPasswordController
{
    private readonly IForgotPassowrdService _forgotPassowrdService;

    public ForgotPasswordController(IForgotPassowrdService forgotPassowrdService)
    {
        _forgotPassowrdService = forgotPassowrdService;
    }

    [HttpPost]
    [Route("forgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromHeader] string email)
    {
        return await _forgotPassowrdService.ForgotPassword(email);
    }

    [HttpPost]
    [Route("tokenVerification")]
    public async Task<ActionResult> TokenVerification([FromBody] TokenValidationDto token)
    {
        return await _forgotPassowrdService.TokenVerification(token);
    }

    [HttpPut]
    [Route("resetPassword")]
    public async Task<ActionResult> ResetPassword([FromHeader] string password, [FromBody] TokenValidationDto token)
    {
        return await _forgotPassowrdService.ResetPassword(password, token);
    }
}