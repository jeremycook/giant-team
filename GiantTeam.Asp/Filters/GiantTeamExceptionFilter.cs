using GiantTeam.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Asp.Filters
{
    public class GiantTeamExceptionFilter : IActionFilter, IOrderedFilter
    {
        // Let other action filters run before this one
        public int Order => int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                // Return binding errors
                context.Result = new ObjectResult(string.Join(" ", context.ModelState.SelectMany(o => o.Value!.Errors.Select(e => e.ErrorMessage.TrimEnd('.') + $" ({o.Key})."))))
                {
                    StatusCode = 400,
                };
            }
        }

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
            else if (context.Exception is ValidationException validationException)
            {
                context.Result = new ObjectResult(validationException.Message)
                {
                    StatusCode = 400,
                };

                context.ExceptionHandled = true;
            }
        }
    }
}
