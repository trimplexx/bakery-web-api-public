using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Interfaces.Admin;

public interface IAdminProductsService
{
    Task<IActionResult> AddProductAsync(ProductDto productDto, NutritionalValueDto nutritionalValueDto, string? token);
    Task<ActionResult<IEnumerable<object>>> GetCategories(List<int> categoryIds);
    Task<ActionResult<IEnumerable<object>>> GetProductsList(GetProductsList getProductsList);
    Task<IActionResult> GetSingleProduct(string productId);
    Task<IActionResult> DeleteProduct(int productId, string? token);
    Task<ActionResult<int>> GetNumberOfProducts(string? searchTerm, List<int>? categories);

    Task<IActionResult> EditProduct(int productId, ProductDto productDto, NutritionalValueDto nutritionalValueDto,
        string? token);
}