using bakery_web_api.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Controllers.Admin;

[Route("api/[controller]")]
[ApiController]
public class AdminOrdersController
{
    private readonly IAdminOrdersService _adminOrdersService;

    public AdminOrdersController(IAdminOrdersService adminOrdersService)
    {
        _adminOrdersService = adminOrdersService;
    }

    [HttpGet]
    [Route("ordersList")]
    public async Task<ActionResult<IEnumerable<object>>> GetOrdersList([FromHeader] int offset,
        [FromHeader] DateTime dateTime, [FromHeader] string? phone, [FromHeader] string? token)
    {
        return await _adminOrdersService.GetOrdersList(offset, dateTime, phone, token);
    }

    [HttpGet]
    [Route("numberOfOrders")]
    public async Task<ActionResult<int>> GetNumberOfOrders([FromHeader] DateTime dateTime)
    {
        return await _adminOrdersService.GetNumberOfOrders(dateTime);
    }

    [HttpPost]
    [Route("changeOrderStatus")]
    public async Task<ActionResult<string>> ChangeOrderStatus([FromHeader] int orderId, [FromHeader] string? token)
    {
        return await _adminOrdersService.ChangeOrderStatus(orderId, token);
    }
}