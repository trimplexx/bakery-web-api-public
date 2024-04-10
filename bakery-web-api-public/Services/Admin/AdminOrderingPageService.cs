using bakery_web_api.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bakery_web_api.Services.Admin;

public class AdminOrderingPageService : IAdminOrderingPageService
{
    private readonly BakeryDbContext _context;

    public AdminOrderingPageService(BakeryDbContext context)
    {
        _context = context;
    }

    public async Task<ActionResult<IEnumerable<object>>> GetProductsToSelect()
    {
        try
        {
            var products = await _context.Products
                .Select(u => new
                {
                    u.ProductId,
                    u.Name
                })
                .ToListAsync();

            return new OkObjectResult(products);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}