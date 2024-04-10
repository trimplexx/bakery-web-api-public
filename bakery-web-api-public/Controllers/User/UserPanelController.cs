using bakery_web_api.Interfaces.User;
using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Controllers.User;

[Route("api/[controller]")]
[ApiController]
public class UserPanelController : ControllerBase
{
    private readonly IUserPanelService _userPanelService;

    public UserPanelController(IUserPanelService userPanelService)
    {
        _userPanelService = userPanelService;
    }

    [HttpGet]
    [Route("numberOfOrders")]
    public async Task<ActionResult<int>> GetNumberOfOrders([FromHeader] string? userId, [FromHeader] string? token)
    {
        return await _userPanelService.GetNumberOfOrders(userId, token);
    }

    [HttpGet]
    [Route("userOrdersHistoryList")]
    public async Task<IActionResult> GetUserOrdersHistoryList([FromHeader] int offset, [FromHeader] string? userId,
        [FromHeader] string? token)
    {
        return await _userPanelService.GetUserOrdersHistoryList(offset, userId, token);
    }

    [HttpPost]
    [Route("changePassword")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest,
        [FromHeader] string? token)
    {
        return await _userPanelService.ChangePassword(changePasswordRequest, token);
    }

    [HttpGet]
    [Route("checkIfPassword")]
    public async Task<ActionResult<bool>> IsUserGotPassword([FromHeader] string? userId, [FromHeader] string? token)
    {
        return await _userPanelService.IsUserGotPassword(userId, token);
    }
}