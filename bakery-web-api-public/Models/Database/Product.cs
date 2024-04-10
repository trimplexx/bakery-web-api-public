namespace bakery_web_api.Models.Database;

public class Product
{
    public int ProductId { get; set; }

    public string? Name { get; set; }

    public string? Image { get; set; }

    public decimal? Price { get; set; }

    public int? Weight { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual ICollection<ProductsAvailability> ProductsAvailabilities { get; set; } = new List<ProductsAvailability>();

    public virtual ICollection<ProductsNutritionalValue> ProductsNutritionalValues { get; set; } = new List<ProductsNutritionalValue>();

    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
}