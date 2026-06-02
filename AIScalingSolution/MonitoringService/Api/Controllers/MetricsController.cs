using AIScaling.Shared.DTOs;
using AIScaling.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using MonitoringService.Application.Interfaces;

namespace MonitoringService.Api.Controllers;

/// <summary>Resource metrics API.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class MetricsController : ControllerBase
{
    private readonly IMetricAppService _service;

    public MetricsController(IMetricAppService service) => _service = service;

    /// <summary>Get all stored metrics (latest 1000).</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ResourceMetricDto>>>> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<IReadOnlyList<ResourceMetricDto>>.Ok(await _service.GetAllAsync(ct)));

    /// <summary>Get metrics filtered by service name.</summary>
    [HttpGet("service/{serviceName}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ResourceMetricDto>>>> GetByService(
        string serviceName,
        CancellationToken ct) =>
        Ok(ApiResponse<IReadOnlyList<ResourceMetricDto>>.Ok(await _service.GetByServiceAsync(serviceName, ct)));
}
