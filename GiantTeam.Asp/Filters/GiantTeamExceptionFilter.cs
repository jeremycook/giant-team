using GiantTeam.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Asp.Filters
{
    public class GiantTeamExceptionFilter : IActionFilter, IOrderedFilter
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
