namespace AIScaling.Shared.DTOs;

/// <summary>Data transfer object for user creation and updates.</summary>
public sealed class CreateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

/// <summary>Data transfer object representing a user.</summary>
public sealed class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>Data transfer object for updating a user.</summary>
public sealed class UpdateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
