using bakery_web_api.Interfaces.User;
using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bakery_web_api.Services.User;

public class ProductsService : IProductsService
{
    private readonly IConfiguration _configuration;
    private readonly BakeryDbContext _context;

    public ProductsService(BakeryDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }


    public async Task<ActionResult<IEnumerable<object>>> GetProductsList(GetProductsList getProductsList,
        DateTime dateTime)
    {
        try
        {
            var blobKey = _configuration.GetSection("BlobContainerString")["BlobKey"];

            var productsQuery = _context.Products
                .Include(p => p.ProductCategories)
                .Select(u => new
                {
                    u.ProductId,
                    u.Name,
                    u.Price,
                    Categories = u.ProductCategories.Select(pc => pc.CategoryId),
                    Image = !string.IsNullOrEmpty(u.Image) ? blobKey + u.Image : string.Empty,
                    AvailableQuantity = _context.ProductsAvailabilities
                        .Where(pa => pa.ProductId == u.ProductId && pa.Date == dateTime)
                        .Select(pa => pa.Quantity - pa.OrderedQuantity)
                        .FirstOrDefault() ?? 0
                });

            var products = await productsQuery.ToListAsync();

            // Jeżeli Categories ma wartości, filtrujemy produkty według kategorii
            if (getProductsList.Categories != null && getProductsList.Categories.Count > 0)
                products = products.Where(p => getProductsList.Categories.All(c => p.Categories.Contains(c))).ToList();

            // Jeżeli searchTerm ma wartość, filtrujemy produkty według nazwy używając LIKE
            if (!string.IsNullOrEmpty(getProductsList.SearchTerm))
                products = products.Where(p => p.Name.Contains(getProductsList.SearchTerm)).ToList();

            // Sortowanie po ProductId
            products = products.OrderBy(p => p.ProductId).ToList();

            // Przeskoczenie odpowiedniej liczby elementów i wzięcie następnych 10
            products = products.Skip(10 * getProductsList.Offset).Take(10).ToList();

            return new OkObjectResult(products);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }


    public async Task<IActionResult> GetProductsData(DateTime dateTime, List<ProductsFromShoppingCard> names)
    {
        try
        {
            var blobKey = _configuration.GetSection("BlobContainerString")["BlobKey"];

            var productNames = names.Select(n => n.Name).ToList();

            var products = await _context.Products
                .Where(p => productNames.Contains(p.Name))
                .ToListAsync();

            if (products == null || products.Count == 0)
                throw new Exception("Nie znaleziono produktów");

            var productsAvailability = new List<object>();

            foreach (var product in products)
            {
                var productAvailability = await _context.ProductsAvailabilities
                    .FirstOrDefaultAsync(pa => pa.ProductId == product.ProductId && pa.Date == dateTime);

                var maxAvailableQuantity = 0;
                if (productAvailability != null)
                {
                    maxAvailableQuantity =
                        (productAvailability.Quantity ?? 0) - (productAvailability.OrderedQuantity ?? 0);
                    maxAvailableQuantity = maxAvailableQuantity < 0 ? 0 : maxAvailableQuantity;
                }

                var matchingName = names.FirstOrDefault(n => n.Name == product.Name);
                var quantity = matchingName?.Quantity ?? 0;

                productsAvailability.Add(new
                {
                    product.Name,
                    Image = !string.IsNullOrEmpty(product.Image) ? blobKey + product.Image : string.Empty,
                    product.Price,
                    MaxAvailableQuantity = maxAvailableQuantity,
                    Quantity = quantity
                });
            }

            return new OkObjectResult(productsAvailability);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}