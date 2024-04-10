using bakery_web_api.Interfaces.User;
using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Controllers.User;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductsService _productsService;

    public ProductsController(IProductsService productsService)
    {
        _productsService = productsService;
    }

    [HttpPost]
    [Route("singleProduct")]
    public async Task<IActionResult> GetSingleProduct([FromHeader] DateTime dateTime,
        [FromBody] List<ProductsFromShoppingCard> names)
    {
        return await _productsService.GetProductsData(dateTime, names);
    }

    [HttpPost]
    [Route("productsList")]
    public async Task<ActionResult<IEnumerable<object>>> GetProductsList(GetProductsList getProductsList,
        [FromHeader] DateTime dateTime)
    {
        return await _productsService.GetProductsList(getProductsList, dateTime);
    }
}