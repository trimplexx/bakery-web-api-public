namespace bakery_web_api.Models.Database;

public class BlackListSession
{
    public int Id { get; set; }

    public string? Token { get; set; }

    public int UserId { get; set; }

    public DateTime ExpiredAt { get; set; }

    public virtual User User { get; set; } = null!;
}
