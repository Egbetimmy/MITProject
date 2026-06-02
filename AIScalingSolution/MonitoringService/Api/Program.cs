using AIScaling.Infrastructure.Extensions;
using AIScaling.Infrastructure.Logging;
using MonitoringService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogLogging("MonitoringService");
builder.Services.AddCommonInfrastructure(builder.Configuration, "MonitoringService");
builder.Services.AddMonitoringInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "Monitoring Service API", Version = "v1" }));

var app = builder.Build();
await app.Services.MigrateAndSeedAsync();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCommonInfrastructure();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("MonitoringService is healthy"));
app.Run();
