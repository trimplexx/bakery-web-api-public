using System.IdentityModel.Tokens.Jwt;
using System.Text;
using bakery_web_api.Interfaces.Common;
using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace bakery_web_api.Services.Common;

public class UserVerifyService : IUserVerifyService
{
    private readonly IConfiguration _configuration;
    private readonly BakeryDbContext _context;

    public UserVerifyService(BakeryDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ActionResult<string>> EmailVerification(TokenValidationDto token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? string.Empty);

        try
        {
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
                tokenHandler.ValidateToken(token.Token, tokenValidationParams, out var validatedToken);

            // Pobranie userId z claimów tokenu
            var userIdClaim = claimsPrincipal.FindFirst("UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                // Znaleziono UserId w claimach - szukanie użytkownika
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

                if (user != null)
                {
                    // Sprawdź, czy token nie wygasł
                    var jwtToken = (JwtSecurityToken)validatedToken;
                    if (jwtToken.ValidTo < DateTime.UtcNow) throw new SecurityTokenExpiredException("Token wygasł");

                    // Aktualizacja stanu weryfikacji użytkownika
                    user.IsVerify = true;
                    await _context.SaveChangesAsync();

                    return "Konto zostało zweryfikowane";
                }

                throw new Exception("Użytkownik nie znaleziony!");
            }

            throw new Exception("Nieprawidłowy token JWT - brak wymaganego claima UserId");
        }
        catch (SecurityTokenExpiredException ex)
        {
            return new BadRequestObjectResult(new { error = "Token wygasł" });
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            return new BadRequestObjectResult(new { error = "Nieprawidłowy podpis tokenu" });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}