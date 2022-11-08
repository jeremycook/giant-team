using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GiantTeam.Asp.UI.Pages
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        public void OnGet()
        {
        }

        public async Task<ActionResult> OnPost()
        {
            if (ModelState.IsValid && User.Identity?.IsAuthenticated == true)
            {
                // TODO: Logout success message
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToPage("/Logout");
            }

            return Page();
        }
    }
}
