using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using bakery_web_api.Controllers.User;
using bakery_web_api.Models.DTO;
using bakery_web_api.Services.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace bakery_web_api.Services.User;

public class ForgotPasswordService : IForgotPassowrdService
{
    private readonly IConfiguration _configuration;
    private readonly BakeryDbContext _context;

    public ForgotPasswordService(BakeryDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<IActionResult> ForgotPassword(string email)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
                return new OkObjectResult($"Jeżeli konto o tym adresie e-mail istnieje, to w skrzynce znajdzie się wiadomość z linkiem resetującym hasło. Sprawdź folder ze spamem.");

            var token = GenerateJwtToken(email);
            var link = _configuration["ConnectionStrings:pageUrl"] + "/odzyskiwanie-hasla/" + token;

            // Aktualizacja tokenu użytkownika w bazie danych
            user.Token = token;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Wysłanie maila
            await SendTokenEmail(email, link);

            return new OkObjectResult($"Jeżeli konto o tym adresie e-mail istnieje, to w skrzynce znajdzie się wiadomość z linkiem resetującym hasło. Sprawdź folder ze spamem.");
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<ActionResult> TokenVerification(TokenValidationDto token)
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

            // Pobranie email z claimów tokenu
            var emailClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            if (emailClaim != null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == emailClaim.Value);
                if (user == null)
                    throw new Exception("Użytkownik o podanym adresie e-mail nie istnieje!");

                // Sprawdź, czy token użytkownika odpowiada tokenowi przesłanemu
                if (user.Token != token.Token)
                    throw new Exception("Token został wykorzystany!");

                // Sprawdź, czy token nie wygasł
                var jwtToken = (JwtSecurityToken)validatedToken;
                if (jwtToken.ValidTo < DateTime.UtcNow) throw new SecurityTokenExpiredException("Token wygasł");
                return new OkObjectResult(emailClaim.Value);
            }

            throw new Exception("Nieprawidłowy token!");
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }


    public async Task<ActionResult> ResetPassword(string password, TokenValidationDto token)
    {
        try
        {
            var emailResult = await TokenVerification(token);
            if (emailResult is OkObjectResult okResult)
            {
                var email = okResult.Value.ToString();
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

                // Zaktualizuj hasło użytkownika
                user.Password = AuthService.HashPassword(password);

                // Usuń token użytkownika
                user.Token = null;

                await _context.SaveChangesAsync();
                return new OkObjectResult("Hasło zostało pomyślnie zmienione");
            }

            return new BadRequestObjectResult(new { error = "Nie udało się zresetować hasła" });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }


    private async Task SendTokenEmail(string email, string link)
    {
        var smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();

        var smtpClient = new SmtpClient(smtpSettings?.SmtpServer)
        {
            Port = smtpSettings.Port,
            Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password),
            EnableSsl = true
        };

        // Tworzenie maila
        var mailMessage = new MailMessage
        {
            From = new MailAddress("kontakt@trzebachleba.pl", "trzebachleba.pl"),
            Subject = "Odzyskiwanie hasła",
            IsBodyHtml = true
        };
        mailMessage.To.Add(email);
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Mails", "ForgotPasswordEmail.html");
        var emailContent = File.ReadAllText(path);

        emailContent = emailContent.Replace("{link}", link);

        mailMessage.Body = emailContent;


        // Wysłanie maila
        Task.Run(() => smtpClient.SendMailAsync(mailMessage));
    }


    private string GenerateJwtToken(string email)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? string.Empty);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("email", email)
                }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            throw new Exception("Bład generowania tokenu JWT", ex);
        }
    }
}