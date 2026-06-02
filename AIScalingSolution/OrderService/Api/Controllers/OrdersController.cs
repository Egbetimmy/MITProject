using AIScaling.Infrastructure.Metrics;
using AIScaling.Shared.DTOs;
using AIScaling.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Interfaces;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderAppService _service;
    private readonly RuntimeMetricsCollector _metrics;

    public OrdersController(IOrderAppService service, RuntimeMetricsCollector metrics)
    {
        _service = service;
        _metrics = metrics;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OrderDto>>>> GetAll(CancellationToken ct)
    {
        _metrics.RecordRequest(0);
        return Ok(ApiResponse<IReadOnlyList<OrderDto>>.Ok(await _service.GetAllAsync(ct)));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(int id, CancellationToken ct)
    {
        _metrics.RecordRequest(0);
        var item = await _service.GetByIdAsync(id, ct);
        return item is null ? NotFound(ApiResponse<OrderDto>.Fail($"Order {id} not found.")) : Ok(ApiResponse<OrderDto>.Ok(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Create([FromBody] CreateOrderDto dto, CancellationToken ct)
    {
        _metrics.RecordRequest(0);
        var item = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, ApiResponse<OrderDto>.Ok(item));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Update(int id, [FromBody] UpdateOrderDto dto, CancellationToken ct)
    {
        _metrics.RecordRequest(0);
        var item = await _service.UpdateAsync(id, dto, ct);
        return item is null ? NotFound(ApiResponse<OrderDto>.Fail($"Order {id} not found.")) : Ok(ApiResponse<OrderDto>.Ok(item));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        _metrics.RecordRequest(0);
        return await _service.DeleteAsync(id, ct) ? NoContent() : NotFound(ApiResponse<object>.Fail($"Order {id} not found."));
    }
}
