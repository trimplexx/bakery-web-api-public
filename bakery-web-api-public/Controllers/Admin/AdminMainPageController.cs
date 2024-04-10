using bakery_web_api.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Controllers.Admin;

[Route("api/[controller]")]
[ApiController]
public class AdminMainPageController
{
    private readonly IAdminMainPageService _adminMainPageService;

    public AdminMainPageController(IAdminMainPageService adminMainPageService)
    {
        _adminMainPageService = adminMainPageService;
    }

    [HttpHead]
    [Route("checkIfAdmin")]
    public async Task<ActionResult> CheckIfAdmin([FromHeader] string? token)
    {
        return await _adminMainPageService.CheckIfAdmin(token);
    }

    [HttpGet]
    [Route("lastDaysSalary")]
    public async Task<ActionResult<object>> GetLastDaysSalary([FromHeader] string? token)
    {
        return await _adminMainPageService.GetLastDaysSalary(token);
    }

    [HttpGet]
    [Route("productsLeft")]
    public async Task<ActionResult<object>> GetProductsLeft([FromHeader] string? token)
    {
        return await _adminMainPageService.GetProductsLeft(token);
    }

    [HttpGet]
    [Route("unfulfilledOrders")]
    public async Task<ActionResult<object>> GetUnfulfilledOrders([FromHeader] string? token)
    {
        return await _adminMainPageService.GetUnfulfilledOrders(token);
    }

    [HttpGet]
    [Route("numberOfOrders")]
    public async Task<ActionResult<int>> GetNumberOfOrders([FromHeader] string? token)
    {
        return await _adminMainPageService.GetNumberOfOrders(token);
    }
}