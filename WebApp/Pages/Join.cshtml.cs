using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebApp.Services;

namespace WebApp.Pages
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
                    await joinService.JoinAsync(new JoinDataModel
                    {
                        DisplayName = Form.DisplayName,
                        Email = Form.Email,
                        Username = Form.Username,
                        Password = Form.Password,
                    });

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

        public class FormModel
        {
            [StringLength(100, MinimumLength = 3)]
            public string DisplayName { get; set; } = default!;

            [EmailAddress]
            [StringLength(200, MinimumLength = 3)]
            public string Email { get; set; } = default!;

            [RegularExpression("^[A-Za-z][A-Za-z0-9]*$")]
            [StringLength(100, MinimumLength = 3)]
            public string Username { get; set; } = default!;

            [DataType(DataType.Password)]
            [StringLength(100, MinimumLength = 10)]
            public string Password { get; set; } = default!;

            [DataType(DataType.Password)]
            [Compare(nameof(Password))]
            public string PasswordConfirmation { get; set; } = default!;
        }
    }
}
