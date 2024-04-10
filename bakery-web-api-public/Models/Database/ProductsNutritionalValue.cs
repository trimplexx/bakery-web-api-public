namespace bakery_web_api.Models.Database;

public class ProductsNutritionalValue
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int? Kj { get; set; }

    public int? Kcal { get; set; }

    public double? Fat { get; set; }

    public double? SaturatedFat { get; set; }

    public double? Carbohydrates { get; set; }

    public double? Sugars { get; set; }

    public double? Proteins { get; set; }

    public double? Salt { get; set; }

    public virtual Product Product { get; set; } = null!;
}