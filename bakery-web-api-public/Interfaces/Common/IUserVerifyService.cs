using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Interfaces.Common;

public interface IUserVerifyService
{
    Task<ActionResult<string>> EmailVerification(TokenValidationDto token);
}