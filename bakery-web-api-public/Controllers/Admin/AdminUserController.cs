using bakery_web_api.Interfaces.Admin;
using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Controllers.Admin;

[Route("api/[controller]")]
[ApiController]
public class AdminUserController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUserController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet]
    [Route("numberOfUsers")]
    public async Task<ActionResult<int>> GetNumberOfUsers()
    {
        return await _adminUserService.GetNumberOfUsers();
    }

    [HttpGet]
    [Route("singleUser")]
    public async Task<IActionResult> GetSingleUser([FromHeader] string userId, [FromHeader] string token)
    {
        return await _adminUserService.GetSingleUser(userId, token);
    }

    [HttpPost]
    [Route("usersList")]
    public async Task<ActionResult> GetUsersList(GetUsersList getUsersList, [FromHeader] string? token)
    {
        return await _adminUserService.GetUsers(getUsersList, token);
    }

    [HttpPut]
    [Route("editUser")]
    public async Task<IActionResult> EditUser(EditUserDto editUserDto, [FromHeader] string? token)
    {
        return await _adminUserService.EditUser(editUserDto, token);
    }

    [HttpDelete]
    [Route("deleteUser")]
    public async Task<IActionResult> DeleteUser([FromHeader] int userId, [FromHeader] string? token)
    {
        return await _adminUserService.DeleteUser(userId, token);
    }
}