using System.Globalization;
using Azure.Storage.Blobs;
using bakery_web_api.Enums;
using bakery_web_api.Helpers;
using bakery_web_api.Interfaces.Admin;
using bakery_web_api.Models.Database;
using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bakery_web_api.Services.Admin;

public class AdminProductsService : IAdminProductsService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IConfiguration _configuration;
    private readonly BakeryDbContext _context;

    public AdminProductsService(string? connectionString, BakeryDbContext context, IConfiguration configuration)
    {
        _blobServiceClient = new BlobServiceClient(connectionString);
        _context = context;
        _configuration = configuration;
    }

    public async Task<IActionResult> AddProductAsync(ProductDto productDto, NutritionalValueDto nutritionalValueDto,
        string? token)
    {
        try
        {
            var result = await TokenVeryfication.TokenAndRankVerify(token, null, _context, _configuration);
            if (result.Result is UnauthorizedObjectResult badRequest)
                return new UnauthorizedObjectResult(badRequest.Value?.ToString());
            if (result.Value == Rank.User)
                return new UnauthorizedObjectResult("Użytkownik nie jest administratorem");


            var existingProduct = _context.Products.FirstOrDefault(p => p.Name == productDto.Name);
            if (existingProduct != null) throw new Exception("Produkt o tej nazwie już istnieje");

            var imageUrl = string.Empty;
            if (productDto.Image != null)
                imageUrl = await UploadFileString(productDto.Image, "bakeryimagesconteiner");

            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Image = imageUrl
            };

            if (Decimal.TryParse(productDto.Price, NumberStyles.Any, CultureInfo.InvariantCulture,out var price))
                product.Price = price;

            if (float.TryParse(productDto.Weight, NumberStyles.Any, CultureInfo.InvariantCulture,out var weightFloat))
                product.Weight = (int)weightFloat;

            // Dodaj nowe powiązania kategorii
            if (productDto.Categories != null)
            {
                foreach (var categoryId in productDto.Categories)
                {
                    product.ProductCategories.Add(new ProductCategory { CategoryId = categoryId });
                }
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var productId = product.ProductId;

            var nutritionalValue = new ProductsNutritionalValue
            {
                ProductId = productId
            };

            if (float.TryParse(nutritionalValueDto.Kj, NumberStyles.Any, CultureInfo.InvariantCulture, out var kjFloat))
                nutritionalValue.Kj = (int)kjFloat;


            if (float.TryParse(nutritionalValueDto.Kcal, NumberStyles.Any, CultureInfo.InvariantCulture,out var kcalFloat))
                nutritionalValue.Kcal = (int)kcalFloat;

            if (double.TryParse(nutritionalValueDto.Fat, NumberStyles.Any, CultureInfo.InvariantCulture,out var fat))
                nutritionalValue.Fat = Math.Round(fat, 2);

            if (double.TryParse(nutritionalValueDto.SaturatedFat,NumberStyles.Any, CultureInfo.InvariantCulture, out var saturatedFat))
                nutritionalValue.SaturatedFat = Math.Round(saturatedFat, 2);

            if (double.TryParse(nutritionalValueDto.Carbohydrates, NumberStyles.Any, CultureInfo.InvariantCulture,out var carbohydrates))
                nutritionalValue.Carbohydrates = Math.Round(carbohydrates, 2);

            if (double.TryParse(nutritionalValueDto.Sugars, NumberStyles.Any, CultureInfo.InvariantCulture,out var sugars))
                nutritionalValue.Sugars = Math.Round(sugars, 2);

            if (double.TryParse(nutritionalValueDto.Proteins, NumberStyles.Any, CultureInfo.InvariantCulture,out var proteins))
                nutritionalValue.Proteins = Math.Round(proteins, 2);

            if (double.TryParse(nutritionalValueDto.Salt, NumberStyles.Any, CultureInfo.InvariantCulture,out var salt))
                nutritionalValue.Salt = Math.Round(salt, 2);

            _context.ProductsNutritionalValues.Add(nutritionalValue);

            await _context.SaveChangesAsync();
            return new OkObjectResult(new { Message = "Produkt został pomyślnie dodany" });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }


    public async Task<ActionResult<IEnumerable<object>>> GetCategories(List<int> categoryIds)
    {
        try
        {
            var productsCategoriesQuery = _context.ProductCategoryNames.AsQueryable();

            // Jeżeli lista identyfikatorów nie jest pusta, filtrujemy kategorie po identyfikatorach
            if (categoryIds != null && categoryIds.Any())
                productsCategoriesQuery = productsCategoriesQuery.Where(pc => categoryIds.Contains(pc.CategoryId));

            var productsCategories = await productsCategoriesQuery
                .Select(u => new
                {
                    u.CategoryId,
                    u.Name
                })
                .ToListAsync();

            return new OkObjectResult(productsCategories);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }


    public async Task<ActionResult<IEnumerable<object>>> GetProductsList(GetProductsList getProductsList)
    {
        try
        {
            var blobKey = _configuration.GetSection("BlobContainerString")["BlobKey"];

            var productsQuery = _context.Products.AsQueryable();

            // Filtruj produkty po kategoriach po stronie serwera
            if (getProductsList.Categories != null && getProductsList.Categories.Count > 0)
                foreach (var category in getProductsList.Categories)
                    productsQuery = productsQuery.Where(p => p.ProductCategories.Any(pc => pc.CategoryId == category));

            if (!string.IsNullOrEmpty(getProductsList.SearchTerm))
                productsQuery = productsQuery.Where(p => EF.Functions.Like(p.Name, $"%{getProductsList.SearchTerm}%"));

            var products = await productsQuery
                .OrderBy(p => p.ProductId)
                .Skip(10 * getProductsList.Offset)
                .Take(10)
                .Select(u => new
                {
                    u.ProductId,
                    u.Name,
                    u.Price,
                    Categories = u.ProductCategories.Select(pc => pc.CategoryId),
                    Image = !string.IsNullOrEmpty(u.Image) ? blobKey + u.Image : string.Empty
                })
                .ToListAsync();

            return new OkObjectResult(products);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }


    public async Task<IActionResult> GetSingleProduct(string productId)
    {
        try
        {
            var blobKey = _configuration.GetSection("BlobContainerString")["BlobKey"];
            var product = await _context.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.CategoryName)
                .FirstOrDefaultAsync(p => p.ProductId == int.Parse(productId));

            if (product == null)
                throw new Exception("Nie znaleziono produktu");

            var nutritionalValues =
                await _context.ProductsNutritionalValues.FirstOrDefaultAsync(n => n.ProductId == int.Parse(productId));
            if (nutritionalValues == null)
                throw new Exception("Nie znaleziono wartości odżywczych produktu");

            var categories = product.ProductCategories.Select(pc => pc.CategoryName.CategoryId).ToList();
            var containerClient = _blobServiceClient.GetBlobContainerClient("bakeryimagesconteiner");
            var blobClient = containerClient.GetBlobClient(product.Image);
            if (!await blobClient.ExistsAsync()) product.Image = null;


            return new OkObjectResult(new
            {
                product.Name,
                Image = !string.IsNullOrEmpty(product.Image) ? blobKey + product.Image : string.Empty,
                product.Price,
                product.Weight,
                Categories = categories,
                product.Description,
                nutritionalValues.Kj,
                nutritionalValues.Kcal,
                nutritionalValues.Fat,
                nutritionalValues.SaturatedFat,
                nutritionalValues.Carbohydrates,
                nutritionalValues.Sugars,
                nutritionalValues.Proteins,
                nutritionalValues.Salt
            });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }


    public async Task<IActionResult> DeleteProduct(int productId, string? token)
    {
        try
        {
            var result = await TokenVeryfication.TokenAndRankVerify(token, null, _context, _configuration);
            if (result.Result is UnauthorizedObjectResult badRequest)
                return new UnauthorizedObjectResult(badRequest.Value?.ToString());
            if (result.Value == Rank.User)
                return new UnauthorizedObjectResult("Użytkownik nie jest administratorem");


            // Sprawdź, czy produkt istnieje w bazie
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null) throw new Exception("Produkt nie istnieje");

            // Usuń produkt z tabeli Product
            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            var containerClient = _blobServiceClient.GetBlobContainerClient("bakeryimagesconteiner");
            var blobClient = containerClient.GetBlobClient(product.Image);
            await blobClient.DeleteIfExistsAsync();

            return new OkObjectResult("Produkt został pomyślnie usunięty");
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<ActionResult<int>> GetNumberOfProducts(string? searchTerm, List<int>? categories)
    {
        try
        {
            IQueryable<Product> query = _context.Products.Include(p => p.ProductCategories);

            // Uwzględnienie searchTerm w zapytaniu, jeśli został podany
            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(p => EF.Functions.Like(p.Name, $"%{searchTerm}%"));

            // Pobierz listę produktów
            var products = await query.ToListAsync();

            // Filtruj produkty po kategoriach po stronie klienta
            if (categories != null && categories.Count > 0)
                products = products.Where(p => categories.All(c => p.ProductCategories.Any(pc => pc.CategoryId == c)))
                    .ToList();

            var numberOfProducts = products.Count;
            var numberOfPages = (numberOfProducts + 9) / 10;

            return new OkObjectResult(numberOfPages);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> EditProduct(int productId, ProductDto productDto,
        NutritionalValueDto nutritionalValueDto, string? token)
    {
        try
        {
            var result = await TokenVeryfication.TokenAndRankVerify(token, null, _context, _configuration);
            if (result.Result is UnauthorizedObjectResult badRequest)
                return new UnauthorizedObjectResult(badRequest.Value?.ToString());
            if (result.Value == Rank.User)
                return new UnauthorizedObjectResult("Użytkownik nie jest administratorem");


            // Pobierz istniejący produkt
            var existingProduct = await _context.Products
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            // Sprawdź czy produkt istnieje
            if (existingProduct == null)
                throw new Exception("Nie znaleziono produktu o podanym ID");

            // Sprawdź, czy zaktualizowana nazwa produktu już istnieje w bazie danych
            var productWithSameName =
                _context.Products.FirstOrDefault(p => p.Name == productDto.Name && p.ProductId != productId);
            if (productWithSameName != null) throw new Exception("Produkt o tej nazwie już istnieje");

            // Usuń obraz z kontenera blobów
            var containerClient = _blobServiceClient.GetBlobContainerClient("bakeryimagesconteiner");
            var blobClient = containerClient.GetBlobClient(existingProduct.Image);
            await blobClient.DeleteIfExistsAsync();

            // Wstaw nowy obraz do kontenera blobów, jeśli jest nowy obraz
            var imageUrl = string.Empty;
            if (productDto.Image != null)
            {
                imageUrl = await UploadFileString(productDto.Image, "bakeryimagesconteiner");
                existingProduct.Image = imageUrl;
            }
            else
            {
                existingProduct.Image = null;
            }

            // Zaktualizuj dane produktu
            existingProduct.Name = productDto.Name;

            if (Decimal.TryParse(productDto.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
            {
                existingProduct.Price = price;
            }
            
            if (Int32.TryParse(productDto.Weight, NumberStyles.Any, CultureInfo.InvariantCulture,out var weight))
            {
                existingProduct.Weight = weight;
            }

            existingProduct.Description = productDto.Description;

// Usuń istniejące powiązania kategorii
            existingProduct.ProductCategories.Clear();

// Dodaj nowe powiązania kategorii
            if (productDto.Categories != null)
            {
                foreach (var categoryId in productDto.Categories)
                {
                    existingProduct.ProductCategories.Add(new ProductCategory { CategoryId = categoryId });
                }
            }

// Zaktualizuj dane wartości odżywczych produktu
            var nutritionalValue = _context.ProductsNutritionalValues.FirstOrDefault(pnv => pnv.ProductId == productId);
            
            if (nutritionalValue != null)
            {
                if (double.TryParse(nutritionalValueDto.Kj,NumberStyles.Any, CultureInfo.InvariantCulture, out var kjFloat))
                    nutritionalValue.Kj = (int)kjFloat;

                if (double.TryParse(nutritionalValueDto.Kcal, NumberStyles.Any, CultureInfo.InvariantCulture,out var kcalFloat))
                    nutritionalValue.Kcal = (int)kcalFloat;

                if (double.TryParse(nutritionalValueDto.Fat, NumberStyles.Any, CultureInfo.InvariantCulture,out var fat))
                    nutritionalValue.Fat = Math.Round(fat, 2);

                if (double.TryParse(nutritionalValueDto.SaturatedFat,NumberStyles.Any, CultureInfo.InvariantCulture, out var saturatedFat))
                    nutritionalValue.SaturatedFat = Math.Round(saturatedFat, 2);

                if (double.TryParse(nutritionalValueDto.Carbohydrates, NumberStyles.Any, CultureInfo.InvariantCulture,out var carbohydrates))
                    nutritionalValue.Carbohydrates = Math.Round(carbohydrates, 2);

                if (double.TryParse(nutritionalValueDto.Sugars, NumberStyles.Any, CultureInfo.InvariantCulture,out var sugars))
                    nutritionalValue.Sugars = Math.Round(sugars, 2);

                if (double.TryParse(nutritionalValueDto.Proteins, NumberStyles.Any, CultureInfo.InvariantCulture,out var proteins))
                    nutritionalValue.Proteins = Math.Round(proteins, 2);

                if (double.TryParse(nutritionalValueDto.Salt, NumberStyles.Any, CultureInfo.InvariantCulture,out var salt))
                    nutritionalValue.Salt = Math.Round(salt, 2);
            }

            // Zapisz zmiany w bazie danych
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { Message = "Pomyślnie zaaktualizowano dane produktu!" });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }

    private async Task<string> UploadFileString(IFormFile file, string containerName)
    {
        try
        {
            var fileNameWithoutSpaces = Path.GetFileNameWithoutExtension(file.FileName).Replace(" ", "");
            var fileExtension = Path.GetExtension(file.FileName);
        
            // Generowanie UUID i dodanie go do nazwy pliku
            var uuid = Guid.NewGuid().ToString();
            var newFileName = $"{fileNameWithoutSpaces}_{uuid}{fileExtension}";

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(newFileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            return blobClient.Name;
        }
        catch (Exception ex)
        {
            throw new Exception("Wystąpił błąd podczas przesyłania zdjęcia.", ex);
        }
    }

}