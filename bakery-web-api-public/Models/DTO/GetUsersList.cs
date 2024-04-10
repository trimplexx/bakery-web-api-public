namespace bakery_web_api.Models.DTO;

public class GetUsersList
{
    public int Offset { get; set; }
    public string? SearchTerm { get; set; }
}