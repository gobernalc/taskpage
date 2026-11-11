using TaskPage.Domain.Enums;

namespace TaskPage.Domain.Entities;

/// <summary>
/// Represents a task assigned to a user. The name avoids the collision with
/// <see cref="System.Threading.Tasks.Task"/>.
/// </summary>
public class TaskItem
{
    /// <summary>Primary key of the task.</summary>
    public int Id { get; set; }

    /// <summary>Required title of the task.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Current state of the task.</summary>
    public TaskState Status { get; set; }

    /// <summary>Identifier of the assigned user.</summary>
    public int UserId { get; set; }

    /// <summary>Navigation property to the assigned user.</summary>
    public User User { get; set; } = null!;

    /// <summary>Creation timestamp in UTC.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Last modification timestamp in UTC, when applicable.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Additional information stored as a JSON document.</summary>
    public string? AdditionalData { get; set; }
}
