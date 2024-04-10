namespace bakery_web_api.Models.DTO;

public class GetProductsList
{
    public int Offset { get; set; }
    public List<int>? Categories { get; set; }
    public string? SearchTerm { get; set; }
}