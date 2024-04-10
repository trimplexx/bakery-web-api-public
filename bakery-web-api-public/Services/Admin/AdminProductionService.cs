using bakery_web_api.Enums;
using bakery_web_api.Helpers;
using bakery_web_api.Interfaces.Admin;
using bakery_web_api.Models.Database;
using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bakery_web_api.Services.Admin;

public class AdminProductionService : IAdminProductionService
{
    private readonly IConfiguration _configuration;
    private readonly BakeryDbContext _context;

    public AdminProductionService(BakeryDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<IActionResult> GetProductsQuantity(DateTime dateTime)
    {
        try
        {
            var products = await _context.Products
                .GroupJoin(_context.ProductsAvailabilities.Where(a => a.Date == dateTime),
                    product => product.ProductId,
                    availability => availability.ProductId,
                    (product, availability) => new { product, availability })
                .SelectMany(x => x.availability.DefaultIfEmpty(),
                    (x, y) => new
                    {
                        x.product.ProductId,
                        x.product.Name,
                        Quantity = y!.Quantity ?? 0,
                        OrderedQuantity = y.OrderedQuantity ?? 0
                    })
                .ToListAsync();

            return new OkObjectResult(products);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> UpdateProductsAvailability(List<ProductAndQuantity> products, DateTime dateTime,
        string? token)
    {
        try
        {
            var result = await TokenVeryfication.TokenAndRankVerify(token, null, _context, _configuration);
            if (result.Result is UnauthorizedObjectResult badRequest)
                return new UnauthorizedObjectResult(badRequest.Value?.ToString());
            if (result.Value == Rank.User)
                return new UnauthorizedObjectResult("Użytkownik nie jest administratorem");


            foreach (var product in products)
            {
                var availability = await _context.ProductsAvailabilities
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.ProductId == product.ProductId && a.Date == dateTime);

                if (availability == null)
                {
                    // Dodaj nowy rekord, jeśli nie istnieje
                    _context.ProductsAvailabilities.Add(new ProductsAvailability
                    {
                        ProductId = product.ProductId,
                        Date = dateTime,
                        Quantity = product.Quantity,
                        OrderedQuantity = 0
                    });
                }
                else
                {
                    // Aktualizuj istniejący rekord
                    if (product.Quantity < availability.OrderedQuantity)
                        return new BadRequestObjectResult(new
                        {
                            error = $"Ilość nie może być mniejsza niż zamówiona ilość ({availability.OrderedQuantity})."
                        });

                    // Aktualizuj istniejący rekord
                    availability.Quantity = product.Quantity;
                    _context.ProductsAvailabilities.Update(availability);
                }
            }

            await _context.SaveChangesAsync();

            return new OkObjectResult("Produkty zostały zaktualizowane pomyślnie.");
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}