using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Persistence;

namespace ProductService.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;
    public ProductRepository(ProductDbContext context) => _context = context;

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync(cancellationToken);

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Products.FindAsync([id], cancellationToken);

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<Product?> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        if (!await _context.Products.AnyAsync(p => p.Id == product.Id, cancellationToken)) return null;
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync([id], cancellationToken);
        if (product is null) return false;
        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
