using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Seed;

public static class OrderDbSeed
{
    public static async Task SeedAsync(OrderDbContext context)
    {
        if (await context.Orders.AnyAsync()) return;
        context.Orders.AddRange(
            new Order { UserId = 1, ProductId = 1, Quantity = 1, TotalAmount = 999.99m, Status = "Completed" },
            new Order { UserId = 2, ProductId = 2, Quantity = 2, TotalAmount = 59.98m, Status = "Pending" },
            new Order { UserId = 3, ProductId = 3, Quantity = 1, TotalAmount = 79.99m, Status = "Shipped" });
        await context.SaveChangesAsync();
    }
}
