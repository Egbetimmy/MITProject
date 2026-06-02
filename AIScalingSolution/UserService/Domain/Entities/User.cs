namespace UserService.Domain.Entities;

/// <summary>User aggregate root.</summary>
public sealed class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
