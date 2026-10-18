using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace WebAPI;

public class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        (int statusCode, string title) = exception switch
        {
            ResourceNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Request conflicts with existing data"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError ? null : exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
