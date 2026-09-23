using System.Text.Json;
using CinemaAbyss.Events.Application.Abstractions;
using CinemaAbyss.Events.Application.Contracts.Responses;
using Microsoft.AspNetCore.Diagnostics;

namespace CinemaAbyss.Events.Presentation.ErrorHandling;

/// <summary>
/// Translates a failure into the <c>Error</c> schema of api-specification.yaml
/// (<c>{"error": "..."}</c>). This is the only place that knows which failure
/// maps to which status code.
/// </summary>
internal sealed class ErrorResponseExceptionHandler(ILogger<ErrorResponseExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            // Unreadable body or a required field missing: the serializer's
            // message names the offending property.
            BadHttpRequestException { InnerException: JsonException json } => (StatusCodes.Status400BadRequest, json.Message),
            BadHttpRequestException badRequest => (badRequest.StatusCode, badRequest.Message),
            EventPublishFailedException publishFailed => (StatusCodes.Status500InternalServerError, publishFailed.Message),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
        };

        if (statusCode >= StatusCodes.Status500InternalServerError && exception is not EventPublishFailedException)
        {
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ErrorResponse(message), cancellationToken);
        return true;
    }
}
