using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Persistence;

/// <summary>EF Core database context for users.</summary>
public sealed class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.FullName).HasMaxLength(256).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
        });
    }
}
