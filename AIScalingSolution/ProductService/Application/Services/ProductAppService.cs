using AIScaling.Shared.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Application.Services;

public sealed class ProductAppService : IProductAppService
{
    private readonly IProductRepository _repository;
    public ProductAppService(IProductRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await _repository.GetAllAsync(cancellationToken)).Select(Map).ToList();

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var p = await _repository.GetByIdAsync(id, cancellationToken);
        return p is null ? null : Map(p);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            CreatedAt = DateTime.UtcNow
        };
        return Map(await _repository.AddAsync(product, cancellationToken));
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null) return null;
        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.Price = dto.Price;
        existing.Stock = dto.Stock;
        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated is null ? null : Map(updated);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default) =>
        _repository.DeleteAsync(id, cancellationToken);

    private static ProductDto Map(Product p) => new()
    {
        Id = p.Id, Name = p.Name, Description = p.Description,
        Price = p.Price, Stock = p.Stock, CreatedAt = p.CreatedAt
    };
}
