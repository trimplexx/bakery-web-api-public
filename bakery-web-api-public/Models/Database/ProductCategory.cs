namespace bakery_web_api.Models.Database;

public class ProductCategory
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int CategoryId { get; set; }
    public ProductCategoryName? CategoryName { get; set; }
}