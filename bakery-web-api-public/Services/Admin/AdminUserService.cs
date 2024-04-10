using bakery_web_api.Enums;
using bakery_web_api.Helpers;
using bakery_web_api.Interfaces.Admin;
using bakery_web_api.Interfaces.Common;
using bakery_web_api.Models.Database;
using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bakery_web_api.Services.Admin;

public class AdminUserService : IAdminUserService
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly BakeryDbContext _context;

    public AdminUserService(BakeryDbContext context, IConfiguration configuration, IAuthService authService)
    {
        _context = context;
        _configuration = configuration;
        _authService = authService;
    }

    public async Task<ActionResult> GetUsers(GetUsersList getUsersList, string? token)
    {
        try
        {
            var result = await TokenVeryfication.TokenAndRankVerify(token, null, _context, _configuration);
            if (result.Result is UnauthorizedObjectResult badRequest)
                return new UnauthorizedObjectResult(badRequest.Value?.ToString());
            if (result.Value == Rank.User)
                return new UnauthorizedObjectResult("Użytkownik nie jest administratorem");

            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(getUsersList.SearchTerm))
                query = query.Where(u =>
                    EF.Functions.Like(u.FirstName, $"%{getUsersList.SearchTerm}%") ||
                    EF.Functions.Like(u.LastName, $"%{getUsersList.SearchTerm}%") ||
                    EF.Functions.Like(u.Phone, $"%{getUsersList.SearchTerm}%") ||
                    EF.Functions.Like(u.Email, $"%{getUsersList.SearchTerm}%")
                );

            var users = await query
                .OrderBy(p => p.UserId)
                .Skip(12 * getUsersList.Offset)
                .Take(12)
                .Select(u => new
                {
                    u.UserId,
                    u.FirstName,
                    u.LastName,
                    u.Phone,
                    u.Email
                })
                .ToListAsync();

            return new OkObjectResult(users);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }


    public async Task<ActionResult<int>> GetNumberOfUsers()
    {
        try
        {
            var numberOfUsers = await _context.Users.CountAsync();
            return new OkObjectResult((numberOfUsers + 11) / 12);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> GetSingleUser(string userId, string? token)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == int.Parse(userId));
            if (user == null)
                throw new Exception("Nie znaleziono użytkownika");

            var result =
                await TokenVeryfication.TokenAndRankVerify(token, int.Parse(userId), _context, _configuration);
            if (result.Result is UnauthorizedObjectResult badRequest)
                return new UnauthorizedObjectResult(badRequest.Value?.ToString());

            string? phoneValue = null;
            if (user.Phone != "") phoneValue = user.Phone?.Substring(3);
            return new OkObjectResult(new
            {
                first_name = user.FirstName,
                last_name = user.LastName,
                email = user.Email,
                phone = phoneValue
            });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> EditUser(EditUserDto editUserDto, string? token)
    {
        try
        {
            string formatedPhone = null;
            // Pobierz użytkownika do edycji
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == editUserDto.UserId);
            if (user == null) throw new Exception("Użytkownik o podanym id nie został znaleziony.");

            var result =
                await TokenVeryfication.TokenAndRankVerify(token, editUserDto.UserId, _context, _configuration);
            if (result.Result is UnauthorizedObjectResult badRequest)
                return new UnauthorizedObjectResult(badRequest.Value?.ToString());

            // Sprawdź, czy podany numer telefonu występuje już u innego użytkownika
            if (!string.IsNullOrEmpty(editUserDto.Phone) &&
                await _context.Users.AnyAsync(u => u.Phone == editUserDto.Phone && u.UserId != editUserDto.UserId))
                throw new Exception("Podany numer telefonu należy do innego użytkownika.");

            // Walidacja długości numeru telefonu
            if (editUserDto.Phone.Length != 9)
                throw new Exception("Numer telefonu musi mieć od 9 cyfr.");
            
            // Sprawdź, czy podany adres email występuje już u innego użytkownika
            if (!string.IsNullOrEmpty(editUserDto.Email) &&
                await _context.Users.AnyAsync(u => u.Email == editUserDto.Email && u.UserId != editUserDto.UserId))
                throw new Exception("Podany adres email należy do innego użytkownika.");

            // Aktualizuj dane użytkownika na podstawie przekazanych wartości
            if (!string.IsNullOrEmpty(editUserDto.FirstName)) user.FirstName = editUserDto.FirstName;

            if (!string.IsNullOrEmpty(editUserDto.LastName)) user.LastName = editUserDto.LastName;

            if (!string.IsNullOrEmpty(editUserDto.Phone))
            {
                var existingUser = await _context.Users
                    .Where(u => u.Phone == "+48" + editUserDto.Phone && u.UserId != editUserDto.UserId)
                    .FirstOrDefaultAsync();

                if (existingUser != null) throw new Exception("Telefon jest już przypisany do jakiegoś konta");

                formatedPhone = "+48" + editUserDto.Phone;

                // Zmień numer telefonu w tabeli Orders
                var ordersToUpdate = await _context.Orders
                    .Where(o => o.Phone == user.Phone) // Znajdź wszystkie zamówienia z tym numerem telefonu
                    .ToListAsync();

                foreach (var order in
                         ordersToUpdate)
                    order.Phone = "+48" + editUserDto.Phone; // Zaktualizuj numer telefonu w zamówieniach
            }

            if (!string.IsNullOrEmpty(editUserDto.Email)) user.Email = editUserDto.Email;
            var newBlacklistEntry = new BlackListSession
            {
                Token = token,
                UserId = editUserDto.UserId,
                ExpiredAt = DateTime.Now.AddMinutes(6)
            };
            _context.BlackListSessions.Add(newBlacklistEntry);
            user.Phone = formatedPhone;
            token = _authService.GenerateJwtToken(user);
            user.Phone = formatedPhone;
            // Zapisz zmienione dane użytkownika
            await _context.SaveChangesAsync();
            return new OkObjectResult(token);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> DeleteUser(int userId, string? token)
    {
        try
        {
            var result = await TokenVeryfication.TokenAndRankVerify(token, null, _context, _configuration);
            if (result.Result is UnauthorizedObjectResult badRequest)
                return new UnauthorizedObjectResult(badRequest.Value?.ToString());
            if (result.Value == Rank.User)
                return new UnauthorizedObjectResult("Użytkownik nie jest administratorem");


            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) throw new Exception("Użytkownik nie istnieje");

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return new OkObjectResult("Użytkownik został pomyślnie usunięty");
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}