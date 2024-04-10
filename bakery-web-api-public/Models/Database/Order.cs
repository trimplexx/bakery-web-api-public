namespace bakery_web_api.Models.Database;

public class Order
{
    public int OrderId { get; set; }

    public string? Phone { get; set; }

    public DateTime? OrderDate { get; set; }

    public int? Status { get; set; }

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}
