using Microsoft.Extensions.Http.Resilience;

namespace ApiGateway.Intelligence;

public static class GatewayIntelligenceExtensions
{
    public static IServiceCollection AddGatewayIntelligence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<GatewayIntelligenceOptions>()
            .Bind(configuration.GetSection(GatewayIntelligenceOptions.SectionName));

        var intelligenceOptions = configuration
            .GetSection(GatewayIntelligenceOptions.SectionName)
            .Get<GatewayIntelligenceOptions>() ?? new GatewayIntelligenceOptions();

        var timeout = TimeSpan.FromSeconds(Math.Max(1, intelligenceOptions.RequestTimeoutSeconds));
        var retryAttempts = Math.Max(0, intelligenceOptions.RetryCount);

        services.AddHttpClient(GatewayIntelligenceClient.MonitoringClientName, client =>
        {
            client.BaseAddress = new Uri(intelligenceOptions.MonitoringBaseUrl.TrimEnd('/') + "/");
            client.Timeout = timeout;
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        })
        .AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = retryAttempts;
            options.TotalRequestTimeout.Timeout = timeout;
            options.AttemptTimeout.Timeout = timeout;
        });

        services.AddHttpClient(GatewayIntelligenceClient.PredictionClientName, client =>
        {
            client.BaseAddress = new Uri(intelligenceOptions.PredictionBaseUrl.TrimEnd('/') + "/");
            client.Timeout = timeout;
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        })
        .AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = retryAttempts;
            options.TotalRequestTimeout.Timeout = timeout;
            options.AttemptTimeout.Timeout = timeout;
        });

        services.AddSingleton<IGatewayIntelligenceClient, GatewayIntelligenceClient>();
        services.AddSingleton<PredictionModelReadinessMonitor>();
        services.AddSingleton<IPredictionModelReadinessMonitor>(sp =>
            sp.GetRequiredService<PredictionModelReadinessMonitor>());
        services.AddHostedService(sp => sp.GetRequiredService<PredictionModelReadinessMonitor>());

        return services;
    }

    public static IApplicationBuilder UseGatewayIntelligence(this IApplicationBuilder app) =>
        app.UseMiddleware<GatewayPredictionMiddleware>();
}
