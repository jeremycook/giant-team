using GiantTeam.UserManagement;
using GiantTeam.UserManagement.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using static GiantTeam.UserManagement.Services.VerifyPasswordService;

namespace GiantTeam.Authentication.Api.Controllers;

[AllowAnonymous]
public class LoginController : ControllerBase
{
    public class LoginInput
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }

        public bool Elevated { get; set; } = false;

        public bool RemainLoggedIn { get; set; } = false;
    }

    [HttpPost("/api/[Controller]")]
    public async Task<OkResult> Post(
        [FromServices] ILogger<LoginController> logger,
        [FromServices] VerifyPasswordService verifyPasswordService,
        [FromServices] IOptions<CookieAuthenticationOptions> cookieAuthenticationOptions,
        [FromServices] BuildSessionUserService buildSessionUserService,
        LoginInput input)
    {
        VerifyPasswordOutput output;
        try
        {
            output = await verifyPasswordService.VerifyUsernameAndPasswordAsync(new VerifyPasswordInput
            {
                Username = input.Username!,
                Password = input.Password!,
            });
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Login failed for {Username} at {UserIp}.",
                input.Username,
                HttpContext.Connection.RemoteIpAddress);
            throw;
        }

        // Build a session user
        DateTime validUntil = DateTime.UtcNow.Add(cookieAuthenticationOptions.Value.ExpireTimeSpan);
        SessionUser sessionUser = await buildSessionUserService.BuildSessionUserAsync(elevated: input.Elevated, output.UserId, validUntil);

        // Create a principal from the session user
        ClaimsPrincipal principal = new(sessionUser.CreateIdentity(PrincipalHelper.AuthenticationTypes.Password));

        AuthenticationProperties properties = new()
        {
            IsPersistent = input.RemainLoggedIn,
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

        logger.LogInformation("Logged in {Username} at {UserIp}.",
            input.Username,
            HttpContext.Connection.RemoteIpAddress);

        return Ok();
    }
}
