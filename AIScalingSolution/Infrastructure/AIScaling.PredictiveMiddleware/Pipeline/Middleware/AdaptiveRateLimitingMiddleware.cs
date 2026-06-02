using System.Text.Json;
using AIScaling.PredictiveMiddleware.Diagnostics;
using AIScaling.PredictiveMiddleware.Mitigation;

namespace AIScaling.PredictiveMiddleware.Pipeline.Middleware;

/// <summary>
/// Token-bucket rate limiting with limits driven by <see cref="IAdaptiveRateLimitPolicyStore"/>.
/// </summary>
public sealed class AdaptiveRateLimitingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly AdaptiveClientRateLimiter _rateLimiter;
    private readonly IPredictiveDiagnosticsCollector _diagnostics;

    public AdaptiveRateLimitingMiddleware(
        RequestDelegate next,
        AdaptiveClientRateLimiter rateLimiter,
        IPredictiveDiagnosticsCollector diagnostics)
    {
        _next = next;
        _rateLimiter = rateLimiter;
        _diagnostics = diagnostics;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var started = Environment.TickCount64;

        var clientKey = ResolveClientKey(context);
        var isAuthenticated = context.User?.Identity?.IsAuthenticated == true;

        if (!_rateLimiter.TryAcquire(clientKey, isAuthenticated, out var retryAfter))
        {
            _diagnostics.RecordThrottledRequest();

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
                context.Response.ContentType = "application/json";

                await JsonSerializer.SerializeAsync(
                        context.Response.Body,
                        new { error = "Rate limit exceeded", posture = "AdaptiveThrottle" },
                        JsonOptions,
                        context.RequestAborted)
                    .ConfigureAwait(false);
            }

            RecordOverhead(started);
            return;
        }

        await _next(context).ConfigureAwait(false);
        RecordOverhead(started);
    }

    private void RecordOverhead(long startedTicks)
    {
        var elapsedMs = (Environment.TickCount64 - startedTicks);
        _diagnostics.RecordMiddlewareOverheadMs(elapsedMs);
    }

    private static string ResolveClientKey(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var correlation) &&
            !string.IsNullOrWhiteSpace(correlation))
        {
            return correlation.ToString();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
