using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Interfaces.User;

public interface IProductsService
{
    Task<ActionResult<IEnumerable<object>>> GetProductsList(GetProductsList getProductsList, DateTime dateTime);
    Task<IActionResult> GetProductsData(DateTime dateTime, List<ProductsFromShoppingCard> names);
}