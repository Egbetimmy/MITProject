using Microsoft.EntityFrameworkCore;
using MonitoringService.Domain.Entities;

namespace MonitoringService.Infrastructure.Persistence;

public sealed class MonitoringDbContext : DbContext
{
    public MonitoringDbContext(DbContextOptions<MonitoringDbContext> options) : base(options) { }
    public DbSet<ResourceMetric> ResourceMetrics => Set<ResourceMetric>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ResourceMetric>(entity =>
        {
            entity.ToTable("ResourceMetrics");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Id).ValueGeneratedOnAdd();
            entity.Property(m => m.ServiceName).HasMaxLength(100).IsRequired();
            entity.Property(m => m.Timestamp).HasColumnType("datetime");
            entity.HasIndex(m => new { m.ServiceName, m.Timestamp });
        });
    }
}
