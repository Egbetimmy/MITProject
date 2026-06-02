using System.Net.Http.Json;
using System.Text.Json;
using AIScaling.Shared.DTOs;
using AIScaling.Shared.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MonitoringService.Application.Interfaces;
using MonitoringService.Domain.Entities;

namespace MonitoringService.Infrastructure.BackgroundServices;

/// <summary>Polls microservices every 30 seconds and persists metrics.</summary>
public sealed class MetricsPollingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MetricsPollingBackgroundService> _logger;

    public MetricsPollingBackgroundService(
        IServiceProvider serviceProvider,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<MetricsPollingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_configuration.GetValue("Monitoring:PollIntervalSeconds", 30));
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await PollAllServicesAsync(stoppingToken);
            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task PollAllServicesAsync(CancellationToken cancellationToken)
    {
        var services = _configuration.GetSection("Monitoring:Services").Get<List<MonitoredServiceConfig>>() ?? [];
        var client = _httpClientFactory.CreateClient("metrics");

        foreach (var svc in services)
        {
            try
            {
                var response = await client.GetAsync($"{svc.Url.TrimEnd('/')}/api/metrics/current", cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to poll {Service}: {Status}", svc.Name, response.StatusCode);
                    continue;
                }

                var json = await response.Content.ReadFromJsonAsync<ApiResponse<ServiceMetricsDto>>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                    cancellationToken);

                if (json?.Data is null) continue;

                using var scope = _serviceProvider.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IMetricRepository>();
                await repo.SaveAsync(new ResourceMetric
                {
                    ServiceName = json.Data.ServiceName,
                    Timestamp = json.Data.Timestamp,
                    CpuUsage = json.Data.CpuUsage,
                    MemoryUsage = json.Data.MemoryUsage,
                    RequestCount = json.Data.RequestCount,
                    ResponseTime = json.Data.ResponseTime
                }, cancellationToken);

                _logger.LogInformation("Saved metrics for {Service}", svc.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling {Service}", svc.Name);
            }
        }
    }

    private sealed class MonitoredServiceConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
