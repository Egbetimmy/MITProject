using System.Text.Json;
using AIScaling.PredictiveMiddleware.Core.CQRS.Abstractions;
using AIScaling.PredictiveMiddleware.Core.State;
using AIScaling.PredictiveMiddleware.Core.Telemetry.Commands;
using AIScaling.PredictiveMiddleware.Diagnostics;
using AIScaling.PredictiveMiddleware.Pipeline.Ingestion;
using Microsoft.Extensions.Options;

namespace AIScaling.PredictiveMiddleware.Pipeline.Middleware;

/// <summary>
/// ASP.NET Core middleware: non-blocking telemetry writes and protective posture gating.
/// </summary>
/// <remarks>
/// <para>
/// <b>Write path (JMeter / k6 safe):</b> Ticks are enqueued via <see cref="ITelemetryIngestionQueue"/>
/// in O(1) time. A background loop drains to Redis—HTTP threads never await network I/O, so
/// synthetic burst scripts do not inflate P99 latency via pipeline lag.
/// </para>
/// <para>
/// <b>Posture gate:</b> <see cref="ISystemStateProvider"/> is read synchronously from memory;
/// only shedding logic runs before <c>_next</c>, keeping Nominal/Alert paths allocation-light.
/// </para>
/// </remarks>
public sealed class PredictiveTrafficMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ISystemStateProvider _stateProvider;
    private readonly ITelemetryIngestionQueue _ingestionQueue;
    private readonly IPredictiveDiagnosticsCollector _diagnostics;
    private readonly PredictiveMiddlewareOptions _options;

    public PredictiveTrafficMiddleware(
        RequestDelegate next,
        ISystemStateProvider stateProvider,
        ITelemetryIngestionQueue ingestionQueue,
        IPredictiveDiagnosticsCollector diagnostics,
        IOptions<PredictiveMiddlewareOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _stateProvider = stateProvider ?? throw new ArgumentNullException(nameof(stateProvider));
        _ingestionQueue = ingestionQueue ?? throw new ArgumentNullException(nameof(ingestionQueue));
        _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task InvokeAsync(
        HttpContext context,
        ICommandHandler<RecordTrafficTickCommand> commandHandler)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandHandler);

        _diagnostics.RecordRequest();
        DispatchTelemetryWrite(context);

        var snapshot = _stateProvider.GetSnapshot();
        var path = context.Request.Path.Value ?? "/";

        if (snapshot.Posture == ProtectivePosture.Critical && IsNonCriticalRoute(path))
        {
            _diagnostics.RecordThrottledRequest();
            await WriteTooManyRequestsAsync(context).ConfigureAwait(false);
            return;
        }

        if (snapshot.Posture == ProtectivePosture.Alert)
        {
            context.Response.Headers[_options.AlertStatusHeaderName] = _options.AlertStatusHeaderValue;
        }

        await _next(context).ConfigureAwait(false);
    }

    /// <summary>
    /// Fire-and-forget enqueue: the live HTTP thread never awaits Redis or the command handler.
    /// </summary>
    private void DispatchTelemetryWrite(HttpContext context)
    {
        var command = new RecordTrafficTickCommand(
            ResolveEndpointRoute(context),
            DateTimeOffset.UtcNow,
            ResolveClientIdentifier(context));

        _ingestionQueue.TryEnqueue(command);
    }

    private bool IsNonCriticalRoute(string path)
    {
        foreach (var prefix in _options.NonCriticalRoutePrefixes)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task WriteTooManyRequestsAsync(HttpContext context)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            error = "Too Many Requests",
            message = "Non-critical endpoint temporarily unavailable while system posture is Critical.",
            posture = nameof(ProtectivePosture.Critical),
            retryAfterSeconds = 30,
        };

        await JsonSerializer
            .SerializeAsync(context.Response.Body, payload, JsonOptions, context.RequestAborted)
            .ConfigureAwait(false);
    }

    private static string ResolveEndpointRoute(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is not null)
        {
            var routePattern = (endpoint as RouteEndpoint)?.RoutePattern.RawText;
            if (!string.IsNullOrWhiteSpace(routePattern))
            {
                return routePattern;
            }

            if (!string.IsNullOrWhiteSpace(endpoint.DisplayName))
            {
                return endpoint.DisplayName;
            }
        }

        return context.Request.Path.Value ?? "/";
    }

    private static string? ResolveClientIdentifier(HttpContext context)
    {
        // k6 commonly sets this header in scripted scenarios.
        if (context.Request.Headers.TryGetValue("X-K6-TestRun", out var k6Run) &&
            !string.IsNullOrWhiteSpace(k6Run))
        {
            return Truncate($"k6:{k6Run}", 64);
        }

        if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var correlation) &&
            !string.IsNullOrWhiteSpace(correlation))
        {
            return Truncate(correlation.ToString(), 64);
        }

        var userAgent = context.Request.Headers.UserAgent.ToString();
        if (userAgent.Contains("k6/", StringComparison.OrdinalIgnoreCase))
        {
            return Truncate($"k6:{userAgent}", 64);
        }

        if (userAgent.Contains("Apache-HttpClient", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("JMeter", StringComparison.OrdinalIgnoreCase))
        {
            return Truncate($"jmeter:{userAgent}", 64);
        }

        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded) &&
            !string.IsNullOrWhiteSpace(forwarded))
        {
            var firstHop = forwarded.ToString().Split(',')[0].Trim();
            return Truncate(firstHop, 64);
        }

        return Truncate(context.Connection.Id, 64);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
