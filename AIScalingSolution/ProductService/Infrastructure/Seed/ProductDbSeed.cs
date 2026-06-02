using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Persistence;

namespace ProductService.Infrastructure.Seed;

public static class ProductDbSeed
{
    public static async Task SeedAsync(ProductDbContext context)
    {
        if (await context.Products.AnyAsync()) return;
        context.Products.AddRange(
            new Product { Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, Stock = 50 },
            new Product { Name = "Mouse", Description = "Wireless mouse", Price = 29.99m, Stock = 200 },
            new Product { Name = "Keyboard", Description = "Mechanical keyboard", Price = 79.99m, Stock = 100 });
        await context.SaveChangesAsync();
    }
}
