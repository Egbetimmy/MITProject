using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;
    public OrderRepository(OrderDbContext context) => _context = context;

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Orders.AsNoTracking().OrderBy(o => o.Id).ToListAsync(cancellationToken);

    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Orders.FindAsync([id], cancellationToken);

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<Order?> UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        if (!await _context.Orders.AnyAsync(o => o.Id == order.Id, cancellationToken)) return null;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FindAsync([id], cancellationToken);
        if (order is null) return false;
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
