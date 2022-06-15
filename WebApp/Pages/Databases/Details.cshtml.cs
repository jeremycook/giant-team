using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Databases
{
    public class DetailsModel : PageModel
    {
        [FromRoute]
        public string DatabaseName { get; set; } = null!;

        //[BindProperty]
        //public CreateDatabaseInput Model { get; set; } = new();

        public void OnGet()
        {
        }

        //public async Task<ActionResult> OnPost()
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var database = await databaseService.CreateDatabase(Model);

        //            return RedirectToPage("Details", new { databaseName = database.DatabaseName });
        //        }
        //        catch (ValidationException ex)
        //        {
        //            ModelState.AddModelError("", ex.Message);
        //        }
        //    }

        //    return Page();
        //}
    }
}
