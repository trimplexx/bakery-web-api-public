using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Interfaces.User;

public interface IUserPanelService
{
    Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest, string? token);
    Task<IActionResult> GetUserOrdersHistoryList(int offset, string? userId, string? token);
    Task<ActionResult<int>> GetNumberOfOrders(string? userId, string? token);
    Task<ActionResult<bool>> IsUserGotPassword(string? userId, string? token);
}