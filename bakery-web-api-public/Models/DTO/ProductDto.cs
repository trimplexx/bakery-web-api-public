namespace bakery_web_api.Models.DTO;

public class ProductDto
{
    public string? Name { get; set; }
    public string? Price { get; set; }
    public string? Weight { get; set; }
    public List<int>? Categories { get; set; }
    public string? Description { get; set; }
    public IFormFile? Image { get; set; }
}