using bakery_web_api.Interfaces.User;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Controllers.User;

[Route("api/[controller]")]
[ApiController]
public class InstagramApiController : ControllerBase
{
    private readonly IInstagramApi _instagramApi;

    public InstagramApiController(IInstagramApi instagramApi)
    {
        _instagramApi = instagramApi;
    }

    [HttpGet]
    [Route("instagramPost")]
    public async Task<IActionResult> GetSingleProduct()
    {
        return await _instagramApi.GetInstagramPosts();
    }
}