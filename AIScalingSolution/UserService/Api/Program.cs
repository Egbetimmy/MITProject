using AIScaling.Infrastructure.Extensions;
using AIScaling.Infrastructure.Logging;
using AIScaling.Infrastructure.Metrics;
using AIScaling.Shared.DTOs;
using AIScaling.Shared.Responses;
using UserService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogLogging("UserService");
builder.Services.AddCommonInfrastructure(builder.Configuration, "UserService");
builder.Services.AddUserInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "User Service API", Version = "v1" });
    var xml = Path.Combine(AppContext.BaseDirectory, "UserService.Api.xml");
    if (File.Exists(xml)) c.IncludeXmlComments(xml);
});

var app = builder.Build();
await app.Services.MigrateAndSeedAsync();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCommonInfrastructure();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("UserService is healthy"));
app.MapGet("/api/metrics/current", (RuntimeMetricsCollector collector) =>
    Results.Ok(ApiResponse<ServiceMetricsDto>.Ok(collector.Collect())));

app.Run();
