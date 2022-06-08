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
        [BindProperty]
        public InputModel Model { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<ActionResult> OnPost([FromServices] LoginService loginService)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var principal = await loginService.LoginAsync(new LoginDataModel
                    {
                        Username = Model.Username,
                        Password = Model.Password,
                    });

                    return SignIn(principal, new() { RedirectUri = Url.Content("~/") }, CookieAuthenticationDefaults.AuthenticationScheme);
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return Page();
        }

        public class InputModel
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
