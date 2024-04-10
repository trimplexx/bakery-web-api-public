using System.IdentityModel.Tokens.Jwt;
using System.Text;
using bakery_web_api.Enums;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace bakery_web_api.Helpers;

public static class TokenVeryfication
{
    public static async Task<ActionResult<Rank>> TokenAndRankVerify(string? token, int? requestUserId,
        BakeryDbContext? context, IConfiguration? configuration)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            if (configuration == null && configuration?["Jwt:SecretKey"] is null)
                return new BadRequestObjectResult("Brak prywatnego klucza tokenu w konfiguracji");

            var key = Encoding.ASCII.GetBytes(configuration["Jwt:SecretKey"]);


            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            // Dekodowanie tokenu
            var claimsPrincipal =
                tokenHandler.ValidateToken(token, tokenValidationParams, out var validatedToken);

            // Sprawdzenie, czy token jest na czarnej liście
            var blacklistedToken = await context.BlackListSessions.FirstOrDefaultAsync(b => b.Token == token);
            if (blacklistedToken != null)
                throw new Exception("Token jest na czarnej liście");

            // Pobranie userId z claimów tokenu
            var userIdClaim = claimsPrincipal.FindFirst("UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                // Znaleziono UserId w claimach - szukanie użytkownika
                var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user != null)
                {
                    if (user.IsAdmin) return Rank.Admin;

                    if (requestUserId != null)
                    {
                        if (user.UserId != requestUserId)
                            return new UnauthorizedObjectResult("Błąd identyfikatora użytkownika.");

                        return Rank.User;
                    }
                }

                return new UnauthorizedObjectResult("Użytkownik o podanym ID nie istnieje!");
            }

            throw new Exception();
        }
        catch (Exception ex)
        {
            return new UnauthorizedObjectResult("Token jest wadliwy");
        }
    }
}