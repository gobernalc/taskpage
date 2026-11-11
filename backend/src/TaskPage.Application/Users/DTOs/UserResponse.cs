namespace TaskPage.Application.Users.DTOs;

/// <summary>
/// Read-only representation of a user returned by the API.
/// </summary>
public class UserResponse
{
    /// <summary>User identifier.</summary>
    public int Id { get; init; }

    /// <summary>Full name of the collaborator.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Normalized email address.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Creation timestamp in UTC.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Last modification timestamp in UTC, when applicable.</summary>
    public DateTime? UpdatedAt { get; init; }
}
