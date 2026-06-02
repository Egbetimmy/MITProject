using AIScaling.PredictiveMiddleware.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFullPredictiveMiddleware(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseFullPredictiveMiddleware();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "StressTest.Api" }));

app.Run();
