using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Linq;
using Tywynh.Domain.Exceptions;

namespace Tywynh.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleAsync(ctx, ex);
        }
    }

    private async Task HandleAsync(HttpContext ctx, Exception ex)
    {
        ctx.Response.ContentType = "application/problem+json";

        var problem = ex switch
        {
            ValidationException ve => new ProblemDetails
            {
                Status = 400,
                Title = "Validation failed",
                Extensions = { ["errors"] = ve.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()) }
            },
            KeyNotFoundException => new ProblemDetails { Status = 404, Title = "Not found" },
            DomainException de => new ProblemDetails { Status = 422, Title = de.Message },
            _ => new ProblemDetails
            {
                Status = 500,
                Title = "An unexpected error occurred",
                Detail = _env.IsDevelopment() ? ex.ToString() : null
            }
        };

        ctx.Response.StatusCode = problem.Status ?? 500;
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem,
            new JsonSerializerOptions { PropertyNamingPolicy = new Tywynh.API.Json.SnakeCaseNamingPolicy() }));
    }
}

