using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagement.Api.Middleware;

/// <summary>Converts exceptions to RFC 7807 problem responses.</summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);
            await WriteProblemAsync(context, HttpStatusCode.NotFound, "Resource not found.", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception.");
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError,
                "An unexpected error occurred.", "An unexpected error occurred. See server logs for details.");
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context, HttpStatusCode status, string title, string detail)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = (int)status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
