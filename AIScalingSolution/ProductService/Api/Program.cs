using AIScaling.Infrastructure.Extensions;
using AIScaling.Infrastructure.Logging;
using AIScaling.Infrastructure.Metrics;
using AIScaling.Shared.DTOs;
using AIScaling.Shared.Responses;
using ProductService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogLogging("ProductService");
builder.Services.AddCommonInfrastructure(builder.Configuration, "ProductService");
builder.Services.AddProductInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "Product Service API", Version = "v1" }));

var app = builder.Build();
await app.Services.MigrateAndSeedAsync();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCommonInfrastructure();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("ProductService is healthy"));
app.MapGet("/api/metrics/current", (RuntimeMetricsCollector c) =>
    Results.Ok(ApiResponse<ServiceMetricsDto>.Ok(c.Collect())));
app.Run();
