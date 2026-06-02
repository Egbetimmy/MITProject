using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public sealed class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Status).HasMaxLength(50);
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
        });
    }
}
