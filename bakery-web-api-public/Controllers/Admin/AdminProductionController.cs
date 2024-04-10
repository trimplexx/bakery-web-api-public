using bakery_web_api.Interfaces.Admin;
using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Controllers.Admin;

[Route("api/[controller]")]
[ApiController]
public class AdminProductionController
{
    private readonly IAdminProductionService _adminProductsService;

    public AdminProductionController(IAdminProductionService adminProductionService)
    {
        _adminProductsService = adminProductionService;
    }

    [HttpGet]
    [Route("productsQuantity")]
    public async Task<IActionResult> GetProductsQuantity([FromHeader] DateTime dateTime)
    {
        return await _adminProductsService.GetProductsQuantity(dateTime);
    }

    [HttpPut]
    [Route("updateProductsAvailability")]
    public async Task<IActionResult> UpdateProductsAvailability([FromBody] List<ProductAndQuantity> products,
        [FromHeader] DateTime dateTime, [FromHeader] string? token)
    {
        return await _adminProductsService.UpdateProductsAvailability(products, dateTime, token);
    }
}