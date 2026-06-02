using AIScaling.Shared.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;

namespace UserService.Application.Services;

/// <summary>User CRUD application logic.</summary>
public sealed class UserAppService : IUserAppService
{
    private readonly IUserRepository _repository;

    public UserAppService(IUserRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _repository.GetAllAsync(cancellationToken);
        return users.Select(Map).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : Map(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            Email = dto.Email,
            FullName = dto.FullName,
            CreatedAt = DateTime.UtcNow
        };
        var created = await _repository.AddAsync(user, cancellationToken);
        return Map(created);
    }

    public async Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null) return null;

        existing.Email = dto.Email;
        existing.FullName = dto.FullName;
        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated is null ? null : Map(updated);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default) =>
        _repository.DeleteAsync(id, cancellationToken);

    private static UserDto Map(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FullName = user.FullName,
        CreatedAt = user.CreatedAt
    };
}
