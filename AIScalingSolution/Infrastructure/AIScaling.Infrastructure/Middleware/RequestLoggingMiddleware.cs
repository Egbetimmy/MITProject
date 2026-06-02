using System.Diagnostics;
using AIScaling.Shared.Logging;
using Microsoft.AspNetCore.Http;

namespace AIScaling.Infrastructure.Middleware;

/// <summary>Logs HTTP requests and responses via <see cref="IScalingLogger"/>.</summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IScalingLogger _logger;

    public RequestLoggingMiddleware(RequestDelegate next, IScalingLogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "/";

        _logger.LogRequest(method, path);

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, $"{method} {path}");
            throw;
        }
        finally
        {
            sw.Stop();
            _logger.LogResponse(method, path, context.Response.StatusCode, sw.Elapsed.TotalMilliseconds);
        }
    }
}
