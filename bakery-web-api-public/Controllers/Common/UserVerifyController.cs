using bakery_web_api.Interfaces.Common;
using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Controllers.Common;

[Route("api/[controller]")]
[ApiController]
public class UserVerifyController
{
    private readonly IUserVerifyService _verifyService;

    public UserVerifyController(IUserVerifyService verifyService)
    {
        _verifyService = verifyService;
    }

    [HttpPost]
    [Route("emailVerification")]
    public async Task<ActionResult<string>> EmailVerification([FromBody] TokenValidationDto token)
    {
        return await _verifyService.EmailVerification(token);
    }
}