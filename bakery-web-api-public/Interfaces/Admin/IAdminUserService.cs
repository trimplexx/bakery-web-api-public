using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Interfaces.Admin;

public interface IAdminUserService
{
    Task<ActionResult> GetUsers(GetUsersList getUsersList, string? token);
    Task<ActionResult<int>> GetNumberOfUsers();
    Task<IActionResult> GetSingleUser(string userId, string? token);
    Task<IActionResult> EditUser(EditUserDto editUserDto, string? token);
    Task<IActionResult> DeleteUser(int userId, string? token);
}