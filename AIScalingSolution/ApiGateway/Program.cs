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

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseFullPredictiveMiddleware();
app.UseGatewayIntelligence();
app.UseCommonInfrastructure();
app.MapReverseProxy();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "ApiGateway" }));
app.Run();
