using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Interfaces.Admin;

public interface IAdminOrdersService
{
    Task<ActionResult<IEnumerable<object>>> GetOrdersList(int offset, DateTime dateTime, string? phone, string? token);
    Task<ActionResult<int>> GetNumberOfOrders(DateTime dateTime);
    Task<ActionResult<string>> ChangeOrderStatus(int orderId, string? token);
}