using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace Tywynh.API.Filters;

public class AdminKeyAuthFilter : IActionFilter
{
    private readonly IConfiguration _config;

    public AdminKeyAuthFilter(IConfiguration config)
    {
        _config = config;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var expectedKey = _config["Moderation:AdminKey"];
        if (string.IsNullOrWhiteSpace(expectedKey))
        {
            context.Result = new StatusCodeResult(503);
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue("X-Admin-Key", out var providedKey)
            || providedKey != expectedKey)
        {
            context.Result = new UnauthorizedResult();
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminKeyAuthAttribute : TypeFilterAttribute
{
    public AdminKeyAuthAttribute() : base(typeof(AdminKeyAuthFilter)) { }
}
