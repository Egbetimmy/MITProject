using AIScaling.PredictiveMiddleware.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFullPredictiveMiddleware(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI();

app.UseFullPredictiveMiddleware();

app.MapGet("/api/diagnostics", (
    AIScaling.PredictiveMiddleware.Core.State.ISystemStateProvider stateProvider,
    AIScaling.PredictiveMiddleware.Diagnostics.IPredictiveDiagnosticsCollector diagnosticsCollector) =>
{
    var state = stateProvider.GetSnapshot();
    var diag = diagnosticsCollector.GetSnapshot();
    return Results.Ok(new
    {
        posture = state.Posture.ToString(),
        postureChangedAt = state.PostureChangedAtUtc,
        metricsEvaluatedAt = state.MetricsEvaluatedAtUtc,
        currentRps = diag.CurrentRequestsPerSecond,
        forecastedRps = diag.ForecastedRequestsPerSecond,
        throttledRequests = diag.ThrottledRequests,
        p99OverheadMs = diag.P99MiddlewareOverheadMs,
        lookaheadScores = state.LookaheadMetricScores
    });
});

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "StressTest.Api" }));

app.Run();
