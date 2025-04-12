using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PasswordManager.Web.Filters;

/// <summary>
/// Attribute for adding model state validation
/// </summary>
public class ValidateModelStateAttribute : ActionFilterAttribute
{
    /// <inheritdoc />
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        if (context.ModelState.IsValid)
        {
            return;
        }

        var entry = context.ModelState.First();
        context.Result = new BadRequestObjectResult(
            $"Parameter ${entry.Key} is invalid: {entry.Value.Errors.First().ErrorMessage}");
    }
}
