using GiantTeam.UserManagement;
using GiantTeam.UserManagement.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using static GiantTeam.UserManagement.Services.VerifyPasswordService;

namespace GiantTeam.Asp.UI.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        [FromQuery]
        public string? ReturnUrl { get; set; }

        [BindProperty]
        public FormModel Form { get; set; } = new();

        public void OnGet(string? username = null)
        {
            Form.Username = username!;
        }

        public async Task<ActionResult> OnPost(
            [FromServices] VerifyPasswordService verifyPasswordService,
            [FromServices] IOptions<CookieAuthenticationOptions> cookieAuthenticationOptions,
            [FromServices] BuildSessionUserService buildSessionUserService)
        {
            if (ModelState.IsValid)
            {
                var output = await verifyPasswordService.VerifyUsernameAndPasswordAsync(new VerifyPasswordInput
                {
                    Username = Form.Username!,
                    Password = Form.Password!,
                });

                // Build a session user
                DateTimeOffset validUntil = DateTimeOffset.UtcNow.Add(cookieAuthenticationOptions.Value.ExpireTimeSpan);
                SessionUser sessionUser = await buildSessionUserService.BuildSessionUserAsync(output.UserId, validUntil);

                // Create a principal from the session user
                ClaimsPrincipal principal = new(sessionUser.CreateIdentity(PrincipalHelper.AuthenticationTypes.Password));

                return SignIn(principal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties()
                {
                    RedirectUri = Url.IsLocalUrl(ReturnUrl) ?
                        ReturnUrl :
                        Url.Page("Profile"),
                }, CookieAuthenticationDefaults.AuthenticationScheme);
            }

            return Page();
        }

        public class FormModel
        {
            [RegularExpression("^[A-Za-z][A-Za-z0-9]*$")]
            [StringLength(100, MinimumLength = 3)]
            public string Username { get; set; } = default!;

            [DataType(DataType.Password)]
            [StringLength(100, MinimumLength = 10)]
            public string Password { get; set; } = default!;
        }
    }
}
