using AIScaling.Shared.DTOs;

namespace OrderService.Application.Interfaces;

public interface IOrderAppService
{
    Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateAsync(CreateOrderDto dto, CancellationToken cancellationToken = default);
    Task<OrderDto?> UpdateAsync(int id, UpdateOrderDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
