using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories;

/// <summary>EF Core user repository.</summary>
public sealed class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;

    public UserRepository(UserDbContext context) => _context = context;

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync(cancellationToken);

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Users.FindAsync([id], cancellationToken);

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Users.AnyAsync(u => u.Id == user.Id, cancellationToken);
        if (!exists) return null;

        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync([id], cancellationToken);
        if (user is null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
