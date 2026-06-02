using AIScaling.Shared.DTOs;

namespace UserService.Application.Interfaces;

/// <summary>User application service contract.</summary>
public interface IUserAppService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
