using AIScaling.Infrastructure.Extensions;
using AIScaling.Infrastructure.Logging;
using AIScaling.PredictiveMiddleware.Extensions;
using ApiGateway.Intelligence;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogLogging("ApiGateway");
builder.Services.AddCommonInfrastructure(builder.Configuration, "ApiGateway");
builder.Services.AddFullPredictiveMiddleware(builder.Configuration);
builder.Services.AddGatewayIntelligence(builder.Configuration);
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "API Gateway", Version = "v1" }));
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
app.UseGatewayIntelligence();
app.UseCommonInfrastructure();

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

app.MapReverseProxy();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "ApiGateway" }));
app.Run();
