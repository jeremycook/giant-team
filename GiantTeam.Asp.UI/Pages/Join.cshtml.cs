using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using GiantTeam.UserManagement.Services;

namespace GiantTeam.Asp.UI.Pages
{
    [AllowAnonymous]
    public class JoinModel : PageModel
    {
        [FromQuery]
        public string? ReturnUrl { get; set; }

        [BindProperty]
        public FormModel Form { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<ActionResult> OnPost([FromServices] JoinService joinService)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await joinService.JoinAsync(Form);

                    // TODO: Success message

                    return RedirectToPage("Login", routeValues: new { username = Form.Username, returnUrl = ReturnUrl });
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError(ex.ValidationResult.MemberNames.Count() == 1 ? ex.ValidationResult.MemberNames.First() : string.Empty, ex.Message);
                }
            }

            return Page();
        }

        public class FormModel : JoinService.JoinInput
        {
            [Display(Order = 10001)]
            [DataType(DataType.Password)]
            [Compare(nameof(Password))]
            public string PasswordConfirmation { get; set; } = default!;
        }
    }
}
