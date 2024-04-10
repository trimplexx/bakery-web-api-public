using System.Net;
using System.Net.Mail;
using System.Text;
using bakery_web_api.Interfaces.Common;
using bakery_web_api.Models.Database;
using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bakery_web_api.Services.Common;

public class OrderingService : IOrderingService
{
    private readonly IConfiguration _configuration;
    private readonly BakeryDbContext _context;

    public OrderingService(BakeryDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<IActionResult> GetProductQuantityLeft(int productId, DateTime dateTime)
    {
        try
        {
            // Znajdź dostępność produktu dla podanej daty
            var productAvailability = await _context.ProductsAvailabilities
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.Date == dateTime);

            // Jeżeli produkt nie jest dostępny, zwróć błąd
            if (productAvailability == null || productAvailability.Quantity - productAvailability.OrderedQuantity <= 0)
                throw new Exception(
                    $"Produkt nie jest dostępny dla podanej daty {dateTime.Day}-{dateTime.Month}-{dateTime.Year}");

            // Oblicz pozostałą ilość produktu
            var quantityLeft = productAvailability.Quantity - productAvailability.OrderedQuantity;

            // Jeżeli ilość jest ujemna, ustaw na 0
            if (quantityLeft <= 0) quantityLeft = 0;

            return new OkObjectResult(quantityLeft);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> MakeOrder(List<ProductNameAndQuantity> products, DateTime dateTime, string phone,
        int status)
    {
        try
        {
            if (products == null || !products.Any())
                throw new Exception("Lista produktów jest pusta. Dodaj co najmniej jeden produkt.");
            var checkedProducts = new HashSet<string>(); // Przechowuje nazwy już sprawdzonych produktów

            if (phone is not null)
                // Sprawdzenie poprawności numeru telefonu
                if (!IsValidPhoneNumber(phone))
                    throw new Exception("Numer telefonu jest nieprawidłowy. Numer powinien zawierać 9 cyfr.");

            // Sprawdzenie, czy dateTime jest równy dzisiejszemu dniu i czy godzina zamówienia jest mniejsza niż 15:50
            if (status == 1 && dateTime.Date == DateTime.Now.Date && DateTime.Now.TimeOfDay > new TimeSpan(16, 45, 0))
                throw new Exception("Zamówienie może zostać złożone na minimum 15 min przed zamknięciem lokalu");

            // Sprawdzenie poprawności statusu i ustawienie na 1, jeśli jest inny niż 1 lub 2
            if (status != 1 && status != 2) status = 1;

            var order = new Order
            {
                OrderDate = dateTime,
                Status = status,
                Phone = string.IsNullOrWhiteSpace(phone) ? null : "+48" + phone
            };
            foreach (var product in products)
            {
                if (product.Quantity < 1)
                    throw new Exception(
                        $"Ilość produktu '{product.Name}' wynosi mniej niż 1 sprawdź swoje zamówienie!");

                if (checkedProducts.Contains(product.Name))
                    throw new Exception($"Produkt '{product.Name}' został podany więcej niż jeden raz");

                checkedProducts.Add(product.Name); // Dodaj nazwę produktu do zbioru sprawdzonych produktów

                var productId = await _context.Products
                    .Where(p => p.Name == product.Name)
                    .Select(p => p.ProductId)
                    .FirstOrDefaultAsync();

                if (productId == 0)
                    throw new Exception($"Podano nazwę produktu '{product.Name}', który nie istnieje w bazie");

                var productAvailability = await _context.ProductsAvailabilities
                    .FirstOrDefaultAsync(pa => pa.ProductId == productId && pa.Date == dateTime);

                if (productAvailability != null)
                {
                    var remainingQuantity =
                        (productAvailability.Quantity ?? 0) - (productAvailability.OrderedQuantity ?? 0);

                    if (remainingQuantity < product.Quantity)
                        throw new Exception(
                            $"Dostępna ilość produktu: '{product.Name}' na datę '{dateTime.ToString("dd.MM.yyyy")}' wynosi {remainingQuantity}");

                    productAvailability.OrderedQuantity += product.Quantity;

                    // Dodaj zamówiony produkt do listy zamówionych produktów w zamówieniu
                    order.OrderProducts.Add(new OrderProduct
                    {
                        ProductId = productId,
                        ProductQuantity = product.Quantity
                    });
                }
                else
                {
                    throw new Exception(
                        $"Brak informacji o dostępności produktu '{product.Name}' na datę '{dateTime.ToString("dd.MM.yyyy")}'");
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Wyszukaj użytkownika po numerze telefonu
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == "+48" + phone);
            if (user != null)
                // Wyślij e-mail z podsumowaniem zamówienia
                if (user.Email != null)
                    await SendOrderSummaryEmail(user.Email, order);

            return new OkObjectResult("Zamówienie zrealizowane pomyślnie");
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<ActionResult<bool>> CheckPhoneMakingOrder(string? phone)
    {
        try
        {
            phone = "+48" + phone;
            // Sprawdzenie istnienia numeru telefonu w tabeli users
            var phoneExists = await _context.Users.AnyAsync(u => u.Phone == phone);
            return new OkObjectResult(phoneExists);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<ActionResult> CancelOrder(int orderId)
    {
        try
        {
            // Sprawdź, czy istnieje zamówienie o podanym numerze OrderId
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new Exception($"Zamówienie o numerze {orderId} nie istnieje.");
            if (order.Status == 2)
                throw new Exception("Zamówienie zostało już zrealizowane.");
            if (order.Status == 3)
                throw new Exception("Zamówienie zostało już anulowane.");
            if (order.OrderDate < DateTime.Today)
                throw new Exception("Zamówienie jest przedawnione.");

            // Zamknięcie lokalu.
            var closingTime =
                new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 16, 0, 0);

            // Sprawdzanie, czy zamówienie jest z dzisiaj i czy jest mniej niż 2 godziny do zamknięcia lokalu
            if (order.OrderDate == DateTime.Today && DateTime.Now > closingTime.AddHours(-2))
                throw new Exception("Zamówienie nie może być anulowane mniej niż 2 godziny przed zamknięciem lokalu.");

            // Usuń w pętli wszystkie produkty, które mają orderId podanego zamówienia
            var orderProducts = await _context.OrderProducts.Where(op => op.OrderId == orderId).ToListAsync();

            foreach (var orderProduct in orderProducts)
            {
                // Znajdź odpowiedni wpis w tabeli ProductsAvailability
                var productAvailability = await _context.ProductsAvailabilities
                    .Where(pa => pa.ProductId == orderProduct.ProductId && pa.Date == order.OrderDate)
                    .FirstOrDefaultAsync();

                if (productAvailability != null) productAvailability.OrderedQuantity -= orderProduct.ProductQuantity;
            }

            _context.OrderProducts.RemoveRange(orderProducts);

// Zmień    status zamówienia na 3
            order.Status = 3;
            _context.Orders.Update(order);

            await _context.SaveChangesAsync();

            // Zmień status zamówienia na 3
            order.Status = 3;
            _context.Orders.Update(order);

            await _context.SaveChangesAsync();
            return new OkObjectResult("Pomyślnie anulowano zamówienie.");
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    private async Task<string> LoadOrderSummaryHtml(Order order)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Mails", "OrderSummary.html");
        var html = await File.ReadAllTextAsync(path);

        // Wstawianie danych zamówienia do HTML
        html = html.Replace("{OrderDate}", order.OrderDate.Value.ToString("dd-MM-yyyy"));
        html = html.Replace("{Phone}", order.Phone);

        var productsHtml = new StringBuilder();
        decimal totalCost = 0;
        foreach (var product in order.OrderProducts)
        {
            var singleProduct = await _context.Products
                .Where(p => p.ProductId == product.ProductId)
                .Select(p => new { p.Name, p.Price })
                .FirstOrDefaultAsync();
            var cost = product.ProductQuantity * singleProduct?.Price;
            totalCost += cost ?? 0;

            productsHtml.AppendLine(
                $"<tr><td style='border: 1px solid black; padding: 14px;'>{singleProduct?.Name}</td><td style='border: 1px solid black; padding: 10px;'>{product.ProductQuantity}</td><td style='border: 1px solid black; padding: 10px;'>{cost} zł</td></tr>");
        }

        html = html.Replace("{Products}", productsHtml.ToString());
        html = html.Replace("{TotalCost}", totalCost.ToString());

        return html;
    }

    private async Task SendOrderSummaryEmail(string email, Order order)
    {
        var smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();

        var smtpClient = new SmtpClient(smtpSettings?.SmtpServer)
        {
            Port = smtpSettings.Port,
            Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password),
            EnableSsl = true
        };

        // Wczytanie podsumowania zamówienia z pliku HTML
        var orderSummary = await LoadOrderSummaryHtml(order);

        // Tworzenie maila
        var mailMessage = new MailMessage
        {
            From = new MailAddress("kontakt@trzebachleba.pl", "trzebachleba.pl"),
            Subject = "Podsumowanie Twojego zamówienia",
            Body = orderSummary,
            IsBodyHtml = true
        };
        mailMessage.To.Add(email);

        // Wysłanie maila
        Task.Run(() => smtpClient.SendMailAsync(mailMessage));
    }

    // Metoda sprawdzająca poprawność numeru telefonu (9 cyfr)
    private bool IsValidPhoneNumber(string phone)
    {
        // Usunięcie ewentualnych znaków specjalnych, spacji, myślników itp.
        var cleanPhoneNumber = new string(phone.Where(char.IsDigit).ToArray());

        return cleanPhoneNumber.Length == 9;
    }
}