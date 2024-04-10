namespace bakery_web_api.Models.Database;

public class ProductsAvailability
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public DateTime? Date { get; set; }

    public int? Quantity { get; set; }

    public int? OrderedQuantity { get; set; }

    public virtual Product Product { get; set; } = null!;
}
