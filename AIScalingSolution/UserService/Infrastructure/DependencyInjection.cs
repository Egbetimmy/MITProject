using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Interfaces;
using UserService.Application.Services;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Repositories;
using UserService.Infrastructure.Seed;

namespace UserService.Infrastructure;

/// <summary>Infrastructure layer DI registration.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddUserInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserAppService, UserAppService>();

        return services;
    }

    public static async Task MigrateAndSeedAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        await context.Database.MigrateAsync();
        await UserDbSeed.SeedAsync(context);
    }
}
