using bakery_web_api.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Controllers.Admin;

[Route("api/[controller]")]
[ApiController]
public class AdminOrderingPageController
{
    private readonly IAdminOrderingPageService _adminOrderingPageService;

    public AdminOrderingPageController(IAdminOrderingPageService adminOrderingPageService)
    {
        _adminOrderingPageService = adminOrderingPageService;
    }

    [HttpGet]
    [Route("productsToSelect")]
    public async Task<ActionResult<IEnumerable<object>>> GetProductsToSelect()
    {
        return await _adminOrderingPageService.GetProductsToSelect();
    }
}