using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using GiantTeam.Services;

namespace WebApp.Pages.Data
{
    public class CreateWorkspaceModel : PageModel
    {
        [BindProperty]
        public CreateWorkspaceInput Model { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<ActionResult> OnPost([FromServices] CreateWorkspaceService databaseService)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await databaseService.CreateWorkspaceAsync(Model);

                    return RedirectToPage("Workspace", new { WorkspaceId = Model.WorkspaceName });
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
