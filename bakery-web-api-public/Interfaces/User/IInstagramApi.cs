using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Interfaces.User;

public interface IInstagramApi
{
    Task<ActionResult> GetInstagramPosts();
}