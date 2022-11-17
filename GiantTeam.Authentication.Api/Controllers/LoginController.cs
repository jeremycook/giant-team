using GiantTeam.UserManagement;
using GiantTeam.UserManagement.Services;
using GiantTeam.WorkspaceAdministration.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public bool RemainLoggedIn { get; set; } = false;
    }

    public class LoginOutput
    {
        public LoginOutput(LoginStatus status)
        {
            Status = status;
        }

        public LoginStatus Status { get; }

        public string? Message { get; init; }
    }

    public enum LoginStatus
    {
        /// <summary>
        /// Authentication failed.
        /// Check the <see cref="LoginOutput.Message"/>.
        /// </summary>
        Problem = 400,

        /// <summary>
        /// Authentication succeeded.
        /// An HttpOnly authentication cookie is in the response that clients can use for authenticated requests.
        /// </summary>
        Success = 200,
    }

    [HttpPost("/api/[Controller]")]
    public async Task<LoginOutput> Post(
        [FromServices] VerifyPasswordService verifyPasswordService,
        [FromServices] IOptions<CookieAuthenticationOptions> cookieAuthenticationOptions,
        [FromServices] BuildSessionUserService buildSessionUserService,
        LoginInput input)
    {
        var output = await verifyPasswordService.VerifyAsync(new VerifyPasswordInput
        {
            Username = input.Username!,
            Password = input.Password!,
        });

        switch (output.Status)
        {
            case VerifyPasswordStatus.Problem:
                return new(LoginStatus.Problem)
                {
                    Message = output.Message,
                };
            case VerifyPasswordStatus.Success:
                // OK, continue
                break;
            default:
                throw new NotSupportedException($"Unsupported {nameof(VerifyPasswordStatus)}: {nameof(output.Status)}.");
        }

        // Build a session user
        DateTimeOffset validUntil = DateTimeOffset.UtcNow.Add(cookieAuthenticationOptions.Value.ExpireTimeSpan);
        SessionUser sessionUser = await buildSessionUserService.BuildAsync(output.UserId!.Value, validUntil);

        // Create a principal from the session user
        ClaimsPrincipal principal = new(sessionUser.CreateIdentity(PrincipalHelper.AuthenticationTypes.Password));

        // TODO: Log authentication success

        AuthenticationProperties properties = new()
        {
            IsPersistent = input.RemainLoggedIn,
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

        return new(LoginStatus.Success);
    }
}
