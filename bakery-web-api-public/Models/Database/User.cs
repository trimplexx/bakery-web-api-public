namespace bakery_web_api.Models.Database;

public class User
{
    public int UserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public bool IsVerify { get; set; }

    public string? Token { get; set; }

    public bool IsAdmin { get; set; }

    public virtual ICollection<BlackListSession> BlackListSessions { get; set; } = new List<BlackListSession>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
