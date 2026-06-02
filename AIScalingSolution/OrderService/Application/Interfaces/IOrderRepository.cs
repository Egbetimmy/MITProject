using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

public interface IOrderRepository
{
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order?> UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
