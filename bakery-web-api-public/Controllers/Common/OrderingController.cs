using bakery_web_api.Interfaces.Common;
using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Controllers.Common;

[Route("api/[controller]")]
[ApiController]
public class OrderingController
{
    private readonly IOrderingService _orderingService;

    public OrderingController(IOrderingService orderingService)
    {
        _orderingService = orderingService;
    }

    [HttpGet]
    [Route("productQuantityLeft")]
    public async Task<IActionResult> GetProductQuantityLeft([FromQuery] int productId, [FromQuery] DateTime dateTime)
    {
        return await _orderingService.GetProductQuantityLeft(productId, dateTime);
    }

    [HttpPost]
    [Route("cancelOrder")] //x
    public async Task<IActionResult> CancelOrder([FromHeader] int orderId)
    {
        return await _orderingService.CancelOrder(orderId);
    }

    [HttpGet]
    [Route("checkPhoneMakingOrder")]
    public async Task<ActionResult<bool>> CheckPhoneMakingOrder([FromHeader] string? phone)
    {
        return await _orderingService.CheckPhoneMakingOrder(phone);
    }

    [HttpPost]
    [Route("makeOrder")]
    public async Task<IActionResult> MakeOrder([FromBody] List<ProductNameAndQuantity> products,
        [FromHeader] DateTime dateTime,
        [FromHeader] int status,
        [FromHeader] string? phone = null
    )
    {
        return await _orderingService.MakeOrder(products, dateTime, phone, status);
    }
}