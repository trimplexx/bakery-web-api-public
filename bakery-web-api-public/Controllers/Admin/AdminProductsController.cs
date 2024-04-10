using bakery_web_api.Interfaces.Admin;
using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Controllers.Admin;

[Route("api/[controller]")]
[ApiController]
public class AdminProductsController : ControllerBase
{
    private readonly IAdminProductsService _adminProductsService;

    public AdminProductsController(IAdminProductsService adminProductsService)
    {
        _adminProductsService = adminProductsService;
    }

    [HttpGet]
    [Route("numberOfProducts")]
    public async Task<ActionResult<int>> GetNumberOfProducts([FromHeader] string? searchTerm,
        [FromHeader] List<int>? categories)
    {
        return await _adminProductsService.GetNumberOfProducts(searchTerm, categories);
    }


    [HttpGet]
    [Route("productsCategories")]
    public async Task<ActionResult<IEnumerable<object>>> GetProductsCategories([FromHeader] List<int> categoryIds)
    {
        return await _adminProductsService.GetCategories(categoryIds);
    }

    [HttpGet]
    [Route("singleProduct")]
    public async Task<IActionResult> GetSingleUser([FromHeader] string productId)
    {
        return await _adminProductsService.GetSingleProduct(productId);
    }

    [HttpPost]
    [Route("productsList")]
    public async Task<ActionResult<IEnumerable<object>>> GetProductsList(GetProductsList getProductsList)
    {
        return await _adminProductsService.GetProductsList(getProductsList);
    }

    [HttpPost("addProduct")]
    public async Task<IActionResult> AddProduct([FromForm] ProductDto productDto,
        [FromForm] NutritionalValueDto nutritionalValueDto, [FromHeader] string? token)
    {
        return await _adminProductsService.AddProductAsync(productDto, nutritionalValueDto, token);
    }

    [HttpPut("editProduct")]
    public async Task<IActionResult> EditProduct([FromHeader] int productId, [FromForm] ProductDto productDto,
        [FromForm] NutritionalValueDto nutritionalValueDto, [FromHeader] string? token)
    {
        return await _adminProductsService.EditProduct(productId, productDto, nutritionalValueDto, token);
    }

    [HttpDelete]
    [Route("deleteProduct")]
    public async Task<IActionResult> DeleteProduct([FromHeader] int productId, [FromHeader] string? token)
    {
        return await _adminProductsService.DeleteProduct(productId, token);
    }
}