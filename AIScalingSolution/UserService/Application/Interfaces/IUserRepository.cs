using UserService.Domain.Entities;

namespace UserService.Application.Interfaces;

/// <summary>User persistence abstraction.</summary>
public interface IUserRepository
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
