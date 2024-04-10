using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Interfaces.Admin;

public interface IAdminOrderingPageService
{
    Task<ActionResult<IEnumerable<object>>> GetProductsToSelect();
}