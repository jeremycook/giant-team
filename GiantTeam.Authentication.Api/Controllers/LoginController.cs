using GiantTeam.Asp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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
        /// Something about the input is invalid.
        /// Clients should present the <see cref="LoginOutput.Message"/>.
        /// </summary>
        InvalidInput = 400,

        /// <summary>
        /// An HttpOnly authentication cookie is in the response.
        /// Clients should send the authentication cookie with requests.
        /// Web clients may need to refresh the web page to be able to use the authentication cookie.
        /// </summary>
        Authenticated = 200,
    }

    [HttpPost("/api/[Controller]")]
    public async Task<LoginOutput> Post(
        [FromServices] PasswordAuthenticationService passwordAuthenticationService,
        LoginInput input)
    {
        if (ModelState.IsValid)
        {
            try
            {
                ClaimsPrincipal principal = await passwordAuthenticationService.AuthenticateAsync(new PasswordAuthenticationInput
                {
                    Username = input.Username!,
                    Password = input.Password!,
                });

                AuthenticationProperties properties = new()
                {
                    IsPersistent = input.RemainLoggedIn,
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

                return new(LoginStatus.Authenticated);
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }

        return new(LoginStatus.InvalidInput)
        {
            Message = string.Join(" ", ModelState.SelectMany(e => e.Value?.Errors ?? Enumerable.Empty<ModelError>()).Select(e => e.ErrorMessage)),
        };
    }
}
