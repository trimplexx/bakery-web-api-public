using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Interfaces.Common;

public interface IOrderingService
{
    Task<IActionResult> GetProductQuantityLeft(int productId, DateTime dateTime);

    Task<IActionResult> MakeOrder(List<ProductNameAndQuantity> products, DateTime dateTime, string phone,
        int status);

    Task<ActionResult<bool>> CheckPhoneMakingOrder(string? phone);
    Task<ActionResult> CancelOrder(int orderId);
}