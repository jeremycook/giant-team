using GiantTeam.ComponentModel;
using GiantTeam.WorkspaceAdministration.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static GiantTeam.WorkspaceAdministration.Services.FetchWorkspaceService;

namespace GiantTeam.Data.Api.Controllers;

public class FetchWorkspaceController : ControllerBase
{
    public class DetailedValidationExceptionFilter : Attribute, IActionFilter, IOrderedFilter
    {
        public int Order => int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is DetailedValidationException detailedValidationException)
            {
                context.Result = new ObjectResult(detailedValidationException.Message)
                {
                    StatusCode = detailedValidationException.StatusCode,
                };

                context.ExceptionHandled = true;
            }
        }
    }

    [DetailedValidationExceptionFilter]
    [HttpPost("/api/[Controller]")]
    public async Task<FetchWorkspaceOutput> Post(
        [FromServices] FetchWorkspaceService fetchWorkspaceService,
        FetchWorkspaceInput input)
    {
            return await fetchWorkspaceService.FetchWorkspaceAsync(input);
        //try
        //{
        //    return await fetchWorkspaceService.FetchWorkspaceAsync(input);
        //}
        //catch (DetailedValidationException ex)
        //{
        //    return StatusCode(ex.StatusCode, new
        //    {
        //        ex.Message,
        //    });
        //}
        //catch (ValidationException ex)
        //{
        //    return BadRequest(new
        //    {
        //        ex.Message,
        //    });
        //}
    }
}
