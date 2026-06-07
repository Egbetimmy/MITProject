using AIScaling.Infrastructure.Extensions;
using AIScaling.Infrastructure.Logging;
using PredictionService.Application.Interfaces;
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
app.MapGet("/health/model", (IPredictionAppService predictionService) =>
{
    if (predictionService.IsModelReady)
    {
        return Results.Ok(new { status = "ready", modelLoaded = true });
    }

    return Results.Json(
        new { status = "not_ready", modelLoaded = false, message = "Train the model via POST /api/predictions/train" },
        statusCode: StatusCodes.Status503ServiceUnavailable);
});
app.Run();
