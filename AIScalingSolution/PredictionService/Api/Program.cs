using AIScaling.Infrastructure.Extensions;
using AIScaling.Infrastructure.Logging;
using PredictionService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogLogging("PredictionService");
builder.Services.AddCommonInfrastructure(builder.Configuration, "PredictionService");
builder.Services.AddPredictionInfrastructure();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "Prediction Service API", Version = "v1" }));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCommonInfrastructure();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("PredictionService is healthy"));
app.Run();
