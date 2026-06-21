using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace Nexus.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.TraceIdentifier;
        var path = context.Request.Path;
        var method = context.Request.Method;
        
        using (_logger.BeginScope(new Dictionary<string, object>
               {
                   ["TraceId"] = traceId
               }))
        {
            _logger.LogInformation("Request started {Method} {Path}", method, path);

            try
            {
                await _next(context);

                _logger.LogInformation("Request finished {Method} {Path} StatusCode {StatusCode}", method, path, context.Response.StatusCode);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.LogWarning(ex, "Validation error occurred");
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument error occurred");
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogInformation(ex, "Resource not found");
                await HandleExceptionAsync(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (AuthenticationFailureException ex)
            {
                _logger.LogInformation(ex, "Auth Failed");
                await HandleExceptionAsync(context, HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "Internal server error");
            }
        }
    }
    
    private static async Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            error = message,
            status = (int)statusCode,
        };

        var json = JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}