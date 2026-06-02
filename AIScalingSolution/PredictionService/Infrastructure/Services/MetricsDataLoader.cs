using AIScaling.Shared.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PredictionService.Application.Interfaces;

namespace PredictionService.Infrastructure.Services;

/// <summary>Loads historical metrics from MonitoringDb for ML training.</summary>
public sealed class MetricsDataLoader : IMetricsDataLoader
{
    private readonly string _connectionString;

    public MetricsDataLoader(IConfiguration configuration) =>
        _connectionString = configuration.GetConnectionString("MonitoringConnection")
            ?? throw new InvalidOperationException("MonitoringConnection is not configured.");

    public async Task<IReadOnlyList<MetricData>> LoadHistoricalMetricsAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<MetricData>();
        const string sql = """
            SELECT TOP 5000 CpuUsage, MemoryUsage, RequestCount, ResponseTime,
                   CAST(RequestCount AS FLOAT) * 1.2 + CpuUsage * 5 AS PredictedRequestLoad
            FROM ResourceMetrics
            ORDER BY Timestamp DESC
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new MetricData
            {
                CpuUsage = (float)reader.GetDouble(0),
                MemoryUsage = (float)reader.GetDouble(1),
                RequestCount = reader.GetInt32(2),
                ResponseTime = (float)reader.GetDouble(3),
                PredictedRequestLoad = (float)reader.GetDouble(4)
            });
        }

        return results;
    }
}
