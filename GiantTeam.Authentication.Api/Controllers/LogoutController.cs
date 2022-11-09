using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiantTeam.Authentication.Api.Controllers;

[AllowAnonymous]
public class LogoutController : ControllerBase
{
    [HttpPost("/api/[Controller]")]
    public async Task<OkResult> Post()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }
}
