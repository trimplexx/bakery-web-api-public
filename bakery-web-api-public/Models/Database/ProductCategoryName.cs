namespace bakery_web_api.Models.Database;

public class ProductCategoryName
{
    public int CategoryId { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
}