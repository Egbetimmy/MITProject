using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Seed;

/// <summary>Sample user seed data.</summary>
public static class UserDbSeed
{
    public static async Task SeedAsync(UserDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        context.Users.AddRange(
            new User { Email = "alice@example.com", FullName = "Alice Johnson" },
            new User { Email = "bob@example.com", FullName = "Bob Smith" },
            new User { Email = "carol@example.com", FullName = "Carol Williams" });

        await context.SaveChangesAsync();
    }
}
