using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebApp.Services;

namespace WebApp.Pages.Databases
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public CreateDatabaseInput Model { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<ActionResult> OnPost([FromServices] CreateDatabaseService databaseService)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var database = await databaseService.CreateDatabase(Model);

                    return RedirectToPage("Details", new { DatabaseName = database.DatabaseName });
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return Page();
        }
    }
}
