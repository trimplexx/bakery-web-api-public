using System.Globalization;
using bakery_web_api.Helpers;
using bakery_web_api.Interfaces.User;
using bakery_web_api.Models.DTO;
using bakery_web_api.Services.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bakery_web_api.Services.User;

public class UserPanelService : IUserPanelService
{
    private readonly IConfiguration _configuration;
    private readonly BakeryDbContext _context;

    public UserPanelService(BakeryDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest, string? token)
    {
        try
        {
            Int32.TryParse(changePasswordRequest.UserId, NumberStyles.Any, CultureInfo.InvariantCulture,out var userIdInt);
            if (changePasswordRequest.UserId is null) throw new Exception("ID użytkownika nie ma wartości!");

            var result = await TokenVeryfication.TokenAndRankVerify(token, userIdInt, _context, _configuration);
            if (result.Result is UnauthorizedObjectResult badRequest)
                return new UnauthorizedObjectResult(badRequest.Value?.ToString());

            var user = await _context.Users.FindAsync(userIdInt);
            if (user == null) throw new Exception("Nie znaleziono użytkownika");
            if (user.Password == null)
                throw new Exception("Użytkownik nie ma hasła, nie można go zmienić.");

            if (!AuthService.CheckPassword(changePasswordRequest.OldPassword, user.Password))
                throw new Exception("Podane stare hasło jest nieprawidłowe");

            user.Password = AuthService.HashPassword(changePasswordRequest.NewPassword);

            await _context.SaveChangesAsync();
            return new OkObjectResult("Hasło zostało pomyślnie zmienione");
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> GetUserOrdersHistoryList(int offset, string? userId, string? token)
    {
        try
        {
            Int32.TryParse(userId, NumberStyles.Any, CultureInfo.InvariantCulture,out var userIdInt);
            if (userId is null) throw new Exception("ID użytkownika nie ma wartości!");

            var result = await TokenVeryfication.TokenAndRankVerify(token, userIdInt, _context, _configuration);
            if (result.Result is UnauthorizedObjectResult badRequest)
                return new UnauthorizedObjectResult(badRequest.Value?.ToString());

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userIdInt);
            if (user is null) throw new Exception("Użytkownik o podanym ID nie istnieje!");

            if (string.IsNullOrEmpty(user.Phone))
                throw new Exception("Użytkownik nie ma przypisanego numeru telefonu!");

            var query = _context.Orders
                .Where(o => o.Phone.Contains(user.Phone))
                .OrderBy(o => o.OrderId)
                .Skip(7 * offset)
                .Take(7);

            var orders = await query
                .Select(o => new
                {
                    o.OrderId,
                    o.Phone,
                    o.Status,
                    o.OrderDate,
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
                    order.Status,
                    FormattedOrderDate =
                        order.OrderDate?.ToString("dd-MM-yyyy"), // Formatowanie daty, uwzględniając nullability
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

    public async Task<ActionResult<int>> GetNumberOfOrders(string? userId, string? token)
    {
        try
        {
            Int32.TryParse(userId,NumberStyles.Any, CultureInfo.InvariantCulture, out var userIdInt);
            if (userId is null) throw new Exception("ID użytkownika nie ma wartości!");

            var result = await TokenVeryfication.TokenAndRankVerify(token, userIdInt, _context, _configuration);
            if (result.Result is UnauthorizedObjectResult badRequest)
                return new UnauthorizedObjectResult(badRequest.Value?.ToString());

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userIdInt);
            if (user is null) throw new Exception("Użytkownik o podanym ID nie istnieje!");

            if (string.IsNullOrEmpty(user.Phone))
                throw new Exception("Użytkownik nie ma przypisanego numeru telefonu!");

            var numberOfOrders = await _context.Orders.CountAsync(o => o.Phone == user.Phone);
            return new OkObjectResult((numberOfOrders + 6) / 7);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<ActionResult<bool>> IsUserGotPassword(string? userId, string? token)
    {
        try
        {
            Int32.TryParse(userId,NumberStyles.Any, CultureInfo.InvariantCulture,out var userIdInt);
            if (userId is null) throw new Exception("ID użytkownika nie ma wartości!");

            var result = await TokenVeryfication.TokenAndRankVerify(token, userIdInt, _context, _configuration);
            if (result.Result is UnauthorizedObjectResult badRequest)
                return new UnauthorizedObjectResult(badRequest.Value?.ToString());

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userIdInt);
            if (user is null) throw new Exception("Użytkownik o podanym ID nie istnieje!");

            return !string.IsNullOrEmpty(user.Password);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}