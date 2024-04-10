using bakery_web_api.Enums;
using bakery_web_api.Helpers;
using bakery_web_api.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bakery_web_api.Services.Admin;

public class AdminOrdersService : IAdminOrdersService
{
    private readonly IConfiguration _configuration;
    private readonly BakeryDbContext _context;

    public AdminOrdersService(BakeryDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ActionResult<int>> GetNumberOfOrders(DateTime dateTime)
    {
        try
        {
            var numberOfOrders = await _context.Orders.CountAsync(o => o.OrderDate == dateTime);
            return new OkObjectResult((numberOfOrders + 9) / 10);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }


    public async Task<ActionResult<IEnumerable<object>>> GetOrdersList(int offset, DateTime dateTime, string? phone,
        string? token)
    {
        try
        {
            var result = await TokenVeryfication.TokenAndRankVerify(token, null, _context, _configuration);
            if (result.Result is UnauthorizedObjectResult badRequest)
                return new UnauthorizedObjectResult(badRequest.Value?.ToString());
            if (result.Value == Rank.User)
                return new UnauthorizedObjectResult("Użytkownik nie jest administratorem");


            var query = _context.Orders
                .Where(o => o.OrderDate == dateTime);

            if (!string.IsNullOrEmpty(phone)) query = query.Where(o => EF.Functions.Like(o.Phone, $"%{phone}%"));

            var orders = await query
                .OrderBy(o => o.OrderId)
                .Skip(11 * offset)
                .Take(11)
                .Select(o => new
                {
                    o.OrderId,
                    o.Phone,
                    o.Status,
                    OrderTotal = o.OrderProducts.Sum(op =>
                        op.Product.Price * op.ProductQuantity)
                })
                .ToListAsync();

            var ordersWithProducts = new List<object>();

            foreach (var order in orders)
            {
                var orderedProducts = await _context.OrderProducts
                    .Where(op => op.OrderId == order.OrderId)
                    .Select(op => new
                    {
                        ProductName = op.Product.Name,
                        op.ProductQuantity
                    })
                    .ToListAsync();

                var orderWithProducts = new
                {
                    order.OrderId,
                    order.Phone,
                    order.Status,
                    order.OrderTotal,
                    OrderedProducts = orderedProducts
                };

                ordersWithProducts.Add(orderWithProducts);
            }

            return new OkObjectResult(ordersWithProducts);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }


    public async Task<ActionResult<string>> ChangeOrderStatus(int orderId, string? token)
    {
        try
        {
            var result = await TokenVeryfication.TokenAndRankVerify(token, null, _context, _configuration);
            if (result.Result is UnauthorizedObjectResult badRequest)
                return new UnauthorizedObjectResult(badRequest.Value?.ToString());
            if (result.Value == Rank.User)
                return new UnauthorizedObjectResult("Użytkownik nie jest administratorem");


            var order = await _context.Orders.FindAsync(orderId);

            if (order == null) throw new Exception("Nie znaleziono zamówienia o podanym identyfikatorze.");

            order.Status = 2;

            await _context.SaveChangesAsync();

            return "Zamówienie zostało oznaczone jako zrealizowane!";
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}