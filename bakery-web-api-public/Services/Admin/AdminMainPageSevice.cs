using bakery_web_api.Enums;
using bakery_web_api.Helpers;
using bakery_web_api.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bakery_web_api.Services.Admin;

public class AdminMainPageService : IAdminMainPageService
{
    private readonly IConfiguration _configuration;
    private readonly BakeryDbContext _context;

    public AdminMainPageService(IConfiguration configuration, BakeryDbContext context)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ActionResult> CheckIfAdmin(string? token)
    {
        try
        {
            var result = await TokenVeryfication.TokenAndRankVerify(token, null, _context, _configuration);
            if (result.Result is UnauthorizedObjectResult)
                throw new Exception();
            if (result.Value == Rank.User)
                throw new Exception();

            return new OkResult();
        }
        catch (Exception ex)
        {
            return new UnauthorizedResult();
        }
    }

    public async Task<ActionResult<object>> GetLastDaysSalary(string? token)
    {
        try
        {
            var result = await TokenVeryfication.TokenAndRankVerify(token, null, _context, _configuration);
            if (result.Result is UnauthorizedObjectResult badRequest)
                return new UnauthorizedObjectResult(badRequest.Value?.ToString());
            if (result.Value == Rank.User)
                return new UnauthorizedObjectResult("Użytkownik nie jest administratorem");

            // Oblicz datę początkową (7 dni wstecz)
            var startDate = DateTime.Today.AddDays(-7).Date;

            // Oblicz datę końcową (1 dzień wstecz)
            var endDate = DateTime.Today.AddDays(-1).Date;

            // Pobierz zamówienia zgodnie z kryteriami daty i statusu
            var orders = await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == 2)
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .ToListAsync();

            var earningsByDay = new Dictionary<string, decimal>();

            // Iteracja przez każdy dzień i sumowanie wartości zamówień
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var formattedDate = date.ToString("yyyy-MM-dd");
                var dayOfWeek = date.ToString("dddd");

                var earningsForDay = orders
                    .Where(o => o.OrderDate?.Date == date)
                    .Sum(o => o.OrderProducts.Sum(op => (op.Product?.Price ?? 0) * op.ProductQuantity));

                earningsByDay[formattedDate] = earningsForDay;
            }

            // Zwróć wyniki jako listę dat z zarobkami
            var results = earningsByDay.Select(entry => new
                    { Date = $"{entry.Key}", Earnings = entry.Value })
                .ToList();

            return new OkObjectResult(results);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<ActionResult<object>> GetProductsLeft(string? token)
    {
        try
        {
            var result = await TokenVeryfication.TokenAndRankVerify(token, null, _context, _configuration);
            if (result.Result is UnauthorizedObjectResult badRequest)
                return new UnauthorizedObjectResult(badRequest.Value?.ToString());
            if (result.Value == Rank.User)
                return new UnauthorizedObjectResult("Użytkownik nie jest administratorem");


            // Pobierz dzisiejszą datę
            var today = DateTime.Today.Date;

            // Pobierz produkty dostępne dzisiaj z ilością pozostałą różną od 0
            var productsLeft = await _context.ProductsAvailabilities
                .Where(pa => pa.Date == today && pa.Quantity - pa.OrderedQuantity != 0)
                .Include(pa => pa.Product)
                .Select(pa => new
                {
                    ProductName = pa.Product.Name,
                    QuantityLeft = pa.Quantity - pa.OrderedQuantity
                })
                .ToListAsync();

            return new OkObjectResult(productsLeft);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<ActionResult<object>> GetUnfulfilledOrders(string? token)
    {
        try
        {
            var result = await TokenVeryfication.TokenAndRankVerify(token, null, _context, _configuration);
            if (result.Result is UnauthorizedObjectResult badRequest)
                return new UnauthorizedObjectResult(badRequest.Value?.ToString());
            if (result.Value == Rank.User)
                return new UnauthorizedObjectResult("Użytkownik nie jest administratorem");


            // Pobierz dzisiejszą datę
            var today = DateTime.Today.Date;

            // Pobierz niezrealizowane zamówienia na dzisiaj
            var unfulfilledOrders = await _context.Orders
                .Where(o => o.OrderDate == today && o.Status == 1)
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .Select(o => new
                {
                    PhoneNumber = o.Phone,
                    OrderedProducts = o.OrderProducts.Select(op => new
                    {
                        ProductName = op.Product.Name,
                        Quantity = op.ProductQuantity,
                        TotalPrice = op.Product.Price * op.ProductQuantity
                    }),
                    TotalOrderPrice = o.OrderProducts.Sum(op => op.Product.Price * op.ProductQuantity)
                })
                .ToListAsync();

            return new OkObjectResult(unfulfilledOrders);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<ActionResult<int>> GetNumberOfOrders(string? token)
    {
        try
        {
            var result = await TokenVeryfication.TokenAndRankVerify(token, null, _context, _configuration);
            if (result.Result is UnauthorizedObjectResult badRequest)
                return new UnauthorizedObjectResult(badRequest.Value?.ToString());
            if (result.Value == Rank.User)
                return new UnauthorizedObjectResult("Użytkownik nie jest administratorem");


            // Pobierz dzisiejszą datę
            var today = DateTime.Today.Date;

            // Zlicz niezrealizowane zamówienia na dzisiaj
            var unfulfilledOrdersCount = await _context.Orders
                .CountAsync(o => o.OrderDate == today && o.Status == 1);

            // Zlicz zrealizowane zamówienia na dzisiaj
            var fulfilledOrdersCount = await _context.Orders
                .CountAsync(o => o.OrderDate == today && o.Status == 2);

            // Suma wszystkich zamówień na dzisiaj
            var totalOrdersCount = unfulfilledOrdersCount + fulfilledOrdersCount;

            return new OkObjectResult(new
            {
                UnfulfilledOrders = unfulfilledOrdersCount, FulfilledOrders = fulfilledOrdersCount,
                TotalOrders = totalOrdersCount
            });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}