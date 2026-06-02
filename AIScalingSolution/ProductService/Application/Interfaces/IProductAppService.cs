using AIScaling.Shared.DTOs;

namespace ProductService.Application.Interfaces;

public interface IProductAppService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
