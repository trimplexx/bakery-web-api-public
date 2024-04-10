using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Interfaces.Admin;

public interface IAdminProductionService
{
    Task<IActionResult> GetProductsQuantity(DateTime dateTime);

    Task<IActionResult> UpdateProductsAvailability([FromBody] List<ProductAndQuantity> products,
        [FromHeader] DateTime dateTime, string? token);
}