using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using bakery_web_api.Interfaces.Common;
using bakery_web_api.Models.Database;
using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace bakery_web_api.Services.Common;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly BakeryDbContext _context;

    public AuthService(BakeryDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        try
        {
            if (await _context.Users.AnyAsync(x => x.Email == registerDto.Email))
                throw new Exception("Użytkownik o podanym adresie e-mail już istnieje");

            if (await _context.Users.AnyAsync(x => x.Phone == "+48" + registerDto.Phone))
                throw new Exception("Użytkownik o podanym numerze telefonu już istnieje");

            registerDto.Password = HashPassword(registerDto.Password);

            var user = new Models.Database.User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Phone = "+48" + registerDto.Phone,
                Email = registerDto.Email,
                Password = registerDto.Password
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var token = GenerateJwtToken(user);
            var link = _configuration["ConnectionStrings:pageUrl"] + "/weryfikacja/" + token;
            // Wysłanie maila
            if (registerDto.Email != null) SendVerificationEmail(registerDto.Email, link);
                user.Token = token;
                
            await _context.SaveChangesAsync();
            return new OkObjectResult("Pomyślnie zarejestrowano!");
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }


    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Email);
            if (user == null) throw new Exception("Błędny login bądź hasło");

            // Sprawdzenie, czy token nie wygasł
            if (!user.IsVerify)
            {
                if (user.Token != null || user.Token != String.Empty)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtToken = tokenHandler.ReadToken(user.Token) as JwtSecurityToken;
                    var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId");
                    if (jwtToken.ValidTo > DateTime.Now || userIdClaim != null)
                        throw new Exception(
                            "Konto jest nieaktywne. Na twoim mailu znajduje się link do aktywacji konta. Sprawdź folder ze spamem.");
                }

                var veryficationToken = GenerateJwtToken(user);
                var link = _configuration["ConnectionStrings:pageUrl"] + "/weryfikacja/" + veryficationToken;
                // Wysłanie maila
                if (user.Email != null) await SendVerificationEmail(user.Email, link);
                    user.Token = veryficationToken;
                await _context.SaveChangesAsync();
                throw new Exception(
                    "Konto jest nieaktywne. Na twój mail wysłany został kolejny link aktywacyjny dla konta. Sprawdź folder ze spamem.");
            }

            if (!CheckPassword(loginDto.Password, user.Password)) throw new Exception("Błędny login bądź hasło");

            var token = GenerateJwtToken(user);
            var refreshToken = Guid.NewGuid();
            var refreshTokenAsString = refreshToken.ToString();
            var expirationTime = DateTime.UtcNow.AddMinutes(30);

            var session = new Session
            {
                Token = refreshTokenAsString,
                UserId = user.UserId,
                ExpiredTime = expirationTime
            };
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { token, refreshTokenAsString });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public string GenerateJwtToken(Models.Database.User user)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ??
                                              throw new Exception("Nieprawidłowy secretKey Tokenu"));
            if (user.Phone == "" || user.Phone == null)
                user.Phone = "";
            else
                user.Phone = user.Phone;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim("Phone", user.Phone),
                    new Claim("isAdmin", user.IsAdmin.ToString())
                }),
                Expires = DateTime.Now.AddMinutes(5),
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

    public async Task<ActionResult> Refresh(SessionTokens refreshTokenDto)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            if (_configuration == null && _configuration?["Jwt:SecretKey"] is null)
                return new BadRequestObjectResult("Brak prywatnego klucza tokenu w konfiguracji");
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);
            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            // Dekodowanie tokenu
            var claimsPrincipal =
                tokenHandler.ValidateToken(refreshTokenDto.Token, tokenValidationParams, out var validatedToken);

            // Sprawdzenie, czy token jest na czarnej liście
            var blacklistedToken =
                await _context.BlackListSessions.FirstOrDefaultAsync(b => b.Token == refreshTokenDto.Token);
            if (blacklistedToken != null)
                throw new Exception();

            // Pobieranie daty wygaśnięcia tokenu
            var jwtToken = validatedToken as JwtSecurityToken;
            var expirationDate = jwtToken.ValidTo;

            if (expirationDate < DateTime.UtcNow)
            {
                var session = await _context.Sessions.FirstOrDefaultAsync(x => x.Token == refreshTokenDto.RefreshToken);
                if (session == null)
                    throw new Exception();

                var user = await _context.Users.FindAsync(session.UserId);

                if (user == null)
                    throw new Exception();

                var newToken = GenerateJwtToken(user);

                refreshTokenDto.Token = newToken;

                if (session.ExpiredTime < DateTime.UtcNow)
                {
                    // Aktualizacja tokena odświeżania
                    session.ExpiredTime = DateTime.UtcNow.AddMinutes(30);
                    var refreshToken = Guid.NewGuid();
                    var refreshTokenAsString = refreshToken.ToString();
                    session.Token = refreshTokenAsString;
                    refreshTokenDto.RefreshToken = refreshTokenAsString;
                }

                // Zapisz zmiany w bazie danych
                await _context.SaveChangesAsync();
            }

            return new OkObjectResult(new { refreshTokenDto.Token, refreshTokenDto.RefreshToken });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult("Nastąpiło wylogowanie");
        }
    }

    public async Task<IActionResult> Logout(SessionTokens sessionTokens)
    {
        try
        {
            if (string.IsNullOrEmpty(sessionTokens.Token))
                throw new Exception("Brak tokena");

            var existingSession =
                await _context.Sessions.FirstOrDefaultAsync(s => s.Token == sessionTokens.RefreshToken);
            if (existingSession != null)
            {
                _context.Sessions.Remove(existingSession);
                await _context.SaveChangesAsync();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            if (_configuration == null && _configuration?["Jwt:SecretKey"] is null)
                return new BadRequestObjectResult("Brak prywatnego klucza tokenu w konfiguracji");
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);
            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            // Dekodowanie tokenu
            var claimsPrincipal =
                tokenHandler.ValidateToken(sessionTokens.Token, tokenValidationParams, out var validatedToken);
            var userIdClaim = claimsPrincipal.FindFirst("UserId");
            int.TryParse(userIdClaim.Value, out var userId);

            var existingBlacklistEntry =
                await _context.BlackListSessions.FirstOrDefaultAsync(b => b.Token == sessionTokens.Token);
            if (existingBlacklistEntry == null)
            {
                var newBlacklistEntry = new BlackListSession
                {
                    Token = sessionTokens.Token,
                    UserId = userId,
                    ExpiredAt = DateTime.Now.AddMinutes(6)
                };
                _context.BlackListSessions.Add(newBlacklistEntry);
                await _context.SaveChangesAsync();
            }

            return new OkObjectResult("Nastąpiło wylogowanie");
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> GmailLogin()
    {
        try
        {
            var clientId = _configuration["OAuth:clientId"] ??
                           throw new InvalidOperationException("ClientId is not set in the configuration");
            var redirectUri = _configuration["OAuth:redirectUri"] ??
                              throw new InvalidOperationException("RedirectUri is not set in the configuration");
            var scope =
                "https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/userinfo.profile";

            var authUrl =
                $"https://accounts.google.com/o/oauth2/v2/auth?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope={scope}";

            return new OkObjectResult(authUrl);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> AuthorizeGmailLogin(string code)
    {
        try
        {
            var clientId = _configuration["OAuth:clientId"] ??
                           throw new InvalidOperationException("ClientId is not set in the configuration");
            var clientSecret = _configuration["OAuth:clientSecret"] ??
                               throw new InvalidOperationException("ClientSecret is not set in the configuration");
            var redirectUri = _configuration["OAuth:redirectUri"] ??
                              throw new InvalidOperationException("RedirectUri is not set in the configuration");

            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");

            var decodedCode = WebUtility.UrlDecode(code);

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "code", decodedCode },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" }
            });

            var response = await httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(responseString))
            {
                // Assuming the response string is a JSON object that contains a property 'token'
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                var token = responseObject.id_token.ToString();
                return await SaveOrUpdateUser(token, "Google");
            }

            throw new Exception("Błędna wiadomość Google");
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> FacebookLogin()
    {
        try
        {
            var appId = _configuration["MetaDev:ID"] ??
                        throw new InvalidOperationException("App id is not set in the configuration");
            var redirectUri = _configuration["MetaDev:redirectUri"] ??
                              throw new InvalidOperationException("RedirectUri is not set in the configuration");
            var scope = "public_profile,email";

            var authUrl =
                $"https://www.facebook.com/v13.0/dialog/oauth?client_id={appId}&redirect_uri={redirectUri}&response_type=code&scope={scope}";

            return new OkObjectResult(authUrl);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> AuthorizeFacebookLogin(string code)
    {
        try
        {
            var appId = _configuration["MetaDev:ID"] ??
                        throw new InvalidOperationException("App id is not set in the configuration");
            var appSecret = _configuration["MetaDev:AppSecret"] ??
                            throw new InvalidOperationException("App secret is not set in the configuration");
            var redirectUri = _configuration["MetaDev:redirectUri"] ??
                              throw new InvalidOperationException("RedirectUri is not set in the configuration");

            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://graph.facebook.com/v13.0/oauth/access_token?client_id={appId}&redirect_uri={redirectUri}&client_secret={appSecret}&code={code}");

            var response = await httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(responseString))
            {
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                var accessToken = responseObject.access_token.ToString();
                return await SaveOrUpdateUser(accessToken, "Facebook");
            }

            throw new Exception("Błędna wiadomość Facebooka");
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    private async Task SendVerificationEmail(string email, string link)
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
            Subject = "Potwierdzenie rejestracji",
            IsBodyHtml = true
        };
        mailMessage.To.Add(email);
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Mails", "EmailVerification.html");
        var emailContent = File.ReadAllText(path);

        emailContent = emailContent.Replace("{link}", link);

        mailMessage.Body = emailContent;

        // Wysłanie maila
        Task.Run(() => smtpClient.SendMailAsync(mailMessage));
    }

    public static string HashPassword(string password)
    {
        try
        {
            // Generowanie soli za pomocą RandomNumberGenerator
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var iterations = 10000;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                var hash = pbkdf2.GetBytes(20);

                // Łączenie soli i hasha
                var hashBytes = new byte[36];
                Buffer.BlockCopy(salt, 0, hashBytes, 0, 16);
                Buffer.BlockCopy(hash, 0, hashBytes, 16, 20);

                // Zwracanie hasha jako stringa
                return Convert.ToBase64String(hashBytes);
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Nieoczekiwany błąd generowania tokenu JWT", ex);
        }
    }

    public static bool CheckPassword(string enteredPassword, string storedHash)
    {
        // Konwersja hasha do tablicy bajtów
        var hashBytes = Convert.FromBase64String(storedHash);

        // Pobieranie soli
        var salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        var iterations = 10000;
        using (var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, iterations, HashAlgorithmName.SHA256))
        {
            var hash = pbkdf2.GetBytes(20);

            // Sprawdzanie, czy hashe są takie same
            for (var i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    return false;

            return true;
        }
    }

    public async Task<IActionResult> SaveOrUpdateUser(string accessToken, string loginProvider)
    {
        try
        {
            string email, firstName, lastName;

            if (loginProvider == "Google")
            {
                // Decode the token
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(accessToken);
                var decodedToken = jsonToken as JwtSecurityToken;

                // Extract user data from the token
                email = decodedToken.Claims.First(claim => claim.Type == "email").Value;
                firstName = decodedToken.Claims.First(claim => claim.Type == "given_name").Value;
                lastName = decodedToken.Claims.First(claim => claim.Type == "family_name").Value;
            }
            else if (loginProvider == "Facebook")
            {
                // Get user data from Facebook
                var httpClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://graph.facebook.com/v13.0/me?fields=name,first_name,last_name,email&access_token={accessToken}");

                var response = await httpClient.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(responseString))
                {
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                    email = responseObject.email.ToString();
                    firstName = responseObject.first_name.ToString();
                    lastName = responseObject.last_name.ToString();
                }
                else
                {
                    throw new Exception("Błędna wiadomość Facebooka");
                }
            }
            else
            {
                throw new Exception("Nieznany dostawca logowania");
            }

            // Check if the user already exists in the database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                // If the user does not exist, create a new user
                user = new Models.Database.User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    IsAdmin = false,
                    IsVerify = true
                };

                _context.Users.Add(user);
            }
            else
            {
                user.Email = email;
                user.FirstName = firstName;
                user.LastName = lastName;
            }

            await _context.SaveChangesAsync();
            var tokenSession = GenerateJwtToken(user);
            var refreshToken = Guid.NewGuid();
            var refreshTokenAsString = refreshToken.ToString();
            var expirationTime = DateTime.Now.AddMinutes(30);

            var session = new Session
            {
                Token = refreshTokenAsString,
                UserId = user.UserId,
                ExpiredTime = expirationTime
            };
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            var link = _configuration["ConnectionStrings:pageUrl"] + "/social-session/" + tokenSession + "/" +
                       refreshTokenAsString;

            return new RedirectResult(link);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}