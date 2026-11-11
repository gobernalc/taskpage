namespace TaskPage.Domain.Entities;

/// <summary>
/// Represents a collaborator that tasks can be assigned to.
/// </summary>
public class User
{
    /// <summary>Primary key of the user.</summary>
    public int Id { get; set; }

    /// <summary>Full name of the collaborator.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Unique, normalized email address (trimmed and lowercased).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Creation timestamp in UTC.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Last modification timestamp in UTC, when applicable.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Tasks assigned to this user.</summary>
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
