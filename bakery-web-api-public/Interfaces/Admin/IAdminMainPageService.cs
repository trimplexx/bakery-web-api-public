using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Interfaces.Admin;

public interface IAdminMainPageService
{
    Task<ActionResult<object>> GetLastDaysSalary(string? token);
    Task<ActionResult<object>> GetProductsLeft(string? token);
    Task<ActionResult<object>> GetUnfulfilledOrders(string? token);
    Task<ActionResult<int>> GetNumberOfOrders(string? token);
    Task<ActionResult> CheckIfAdmin(string? token);
}