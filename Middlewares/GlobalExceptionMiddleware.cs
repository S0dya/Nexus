using System.Net;
using System.Text.Json;
using Nexus.Infrastructure.Exceptions;

namespace Nexus.Middlewares;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.TraceIdentifier;
        var path = context.Request.Path;
        var method = context.Request.Method;
        
        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["TraceId"] = traceId
               }))
        {
            // logger.LogInformation("Request started {Method} {Path}", method, path);

            try
            {
                await next(context);

                logger.LogInformation("Request finished {Method} {Path} StatusCode {StatusCode}", method, path, context.Response.StatusCode);
            }
            catch (ApiException ex)
            {
                logger.LogWarning(ex, "Api error occurred on {Method} {Path}", method, path);
                context.Response.StatusCode = ex.StatusCode;
                await HandleExceptionAsync(context, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Unhandled exception occurred on {Method} {Path}", method, path);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await HandleExceptionAsync(context, "Internal server error");
            }
        }
    }
    
    private static async Task HandleExceptionAsync(HttpContext context, string message)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = message,
            status = context.Response.StatusCode,
            traceId = context.TraceIdentifier,
            path = context.Request.Path,
            timestamp = DateTime.UtcNow,
        };

        var json = JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}