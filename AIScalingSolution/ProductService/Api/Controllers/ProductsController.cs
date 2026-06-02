using AIScaling.Infrastructure.Metrics;
using AIScaling.Shared.DTOs;
using AIScaling.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Interfaces;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductAppService _service;
    private readonly RuntimeMetricsCollector _metrics;

    public ProductsController(IProductAppService service, RuntimeMetricsCollector metrics)
    {
        _service = service;
        _metrics = metrics;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProductDto>>>> GetAll(CancellationToken ct)
    {
        _metrics.RecordRequest(0);
        return Ok(ApiResponse<IReadOnlyList<ProductDto>>.Ok(await _service.GetAllAsync(ct)));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(int id, CancellationToken ct)
    {
        _metrics.RecordRequest(0);
        var item = await _service.GetByIdAsync(id, ct);
        return item is null ? NotFound(ApiResponse<ProductDto>.Fail($"Product {id} not found.")) : Ok(ApiResponse<ProductDto>.Ok(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductDto dto, CancellationToken ct)
    {
        _metrics.RecordRequest(0);
        var item = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, ApiResponse<ProductDto>.Ok(item));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(int id, [FromBody] UpdateProductDto dto, CancellationToken ct)
    {
        _metrics.RecordRequest(0);
        var item = await _service.UpdateAsync(id, dto, ct);
        return item is null ? NotFound(ApiResponse<ProductDto>.Fail($"Product {id} not found.")) : Ok(ApiResponse<ProductDto>.Ok(item));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        _metrics.RecordRequest(0);
        return await _service.DeleteAsync(id, ct) ? NoContent() : NotFound(ApiResponse<object>.Fail($"Product {id} not found."));
    }
}
