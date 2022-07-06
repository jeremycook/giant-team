using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebApp.Services;

namespace WebApp.Pages
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
            [FromServices] PasswordAuthenticationService passwordAuthenticationService)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var principal = await passwordAuthenticationService.AuthenticateAsync(new PasswordAuthenticationInput
                    {
                        Username = Form.Username,
                        Password = Form.Password,
                    });

                    return SignIn(principal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties()
                    {
                        RedirectUri = Url.IsLocalUrl(ReturnUrl) ?
                            ReturnUrl :
                            Url.Page("Profile"),
                    }, CookieAuthenticationDefaults.AuthenticationScheme);
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
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
