using AIScaling.Shared.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Application.Services;

public sealed class OrderAppService : IOrderAppService
{
    private readonly IOrderRepository _repository;
    private const decimal UnitPricePlaceholder = 50m;

    public OrderAppService(IOrderRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await _repository.GetAllAsync(cancellationToken)).Select(Map).ToList();

    public async Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var o = await _repository.GetByIdAsync(id, cancellationToken);
        return o is null ? null : Map(o);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto, CancellationToken cancellationToken = default)
    {
        var order = new Order
        {
            UserId = dto.UserId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            TotalAmount = dto.Quantity * UnitPricePlaceholder,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        return Map(await _repository.AddAsync(order, cancellationToken));
    }

    public async Task<OrderDto?> UpdateAsync(int id, UpdateOrderDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null) return null;
        existing.Quantity = dto.Quantity;
        existing.Status = dto.Status;
        existing.TotalAmount = dto.Quantity * UnitPricePlaceholder;
        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated is null ? null : Map(updated);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default) =>
        _repository.DeleteAsync(id, cancellationToken);

    private static OrderDto Map(Order o) => new()
    {
        Id = o.Id, UserId = o.UserId, ProductId = o.ProductId,
        Quantity = o.Quantity, TotalAmount = o.TotalAmount,
        Status = o.Status, CreatedAt = o.CreatedAt
    };
}
