using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Interfaces;
using ProductService.Application.Services;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Repositories;
using ProductService.Infrastructure.Seed;

namespace ProductService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProductInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProductDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductAppService, ProductAppService>();
        return services;
    }

    public static async Task MigrateAndSeedAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        await context.Database.MigrateAsync();
        await ProductDbSeed.SeedAsync(context);
    }
}
